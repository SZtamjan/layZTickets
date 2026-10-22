using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using layZTickets.DataSets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

// A single request scope shares this context with the Identity stores.
// Transactions keep the account, role, category and assignments consistent.
public sealed class AccountAdministration(AppDbContext db,
    UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
{
    public async Task<IdentityResult> CreateAsync(UserData data, ClaimsPrincipal principal)
    {
        if ((await TicketAccess.GetActorAsync(db, principal))?.IsAdmin != true)
            return Failure("Forbidden", "Nie masz uprawnień do tworzenia kont.");

        data.Email = (data.Email ?? string.Empty).Trim();
        data.FirstName = (data.FirstName ?? string.Empty).Trim();
        data.LastName = (data.LastName ?? string.Empty).Trim();
        var validation = Validate(data);
        if (!validation.Succeeded) return validation;

        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new ApplicationUser
        {
            UserName = data.Email, Email = data.Email, FirstName = data.FirstName, LastName = data.LastName,
            AgentCategory = data.Role == AccountRole.Agent ? data.AgentCategory : null
        };
        var result = await users.CreateAsync(user, data.Password);
        if (!result.Succeeded) return result;

        if (data.Role == AccountRole.Agent)
        {
            result = await EnsureAgentRoleAsync();
            if (!result.Succeeded) return result;
            result = await users.AddToRoleAsync(user, "Agent");
            if (!result.Succeeded) return result;
        }

        await transaction.CommitAsync();
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> SetRoleAsync(AccountRoleData data, ClaimsPrincipal principal)
    {
        if ((await TicketAccess.GetActorAsync(db, principal))?.IsAdmin != true)
            return Failure("Forbidden", "Nie masz uprawnień do zmiany ról.");
        var validation = Validate(data);
        if (!validation.Succeeded) return validation;

        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await users.FindByIdAsync(data.AccountId);
        if (user is null) return Failure("AccountMissing", "Wybrane konto już nie istnieje.");
        if (await users.IsInRoleAsync(user, "Admin"))
            return Failure("ProtectedAdministrator", "Ten formularz nie zmienia uprawnień administratorów.");

        user.AgentCategory = data.Role == AccountRole.Agent ? data.AgentCategory : null;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) return result;

        var isAgent = await users.IsInRoleAsync(user, "Agent");
        if (data.Role == AccountRole.Agent && !isAgent)
        {
            result = await EnsureAgentRoleAsync();
            if (!result.Succeeded) return result;
            result = await users.AddToRoleAsync(user, "Agent");
        }
        else if (data.Role == AccountRole.User && isAgent)
        {
            result = await users.RemoveFromRoleAsync(user, "Agent");
        }
        if (!result.Succeeded) return result;

        // An agent must not remain assigned to tickets they can no longer handle.
        var invalidAssignments = await db.Tickets.Where(ticket => ticket.AssignedAgentId == user.Id &&
            (user.AgentCategory == null || ticket.Category != user.AgentCategory)).ToListAsync();
        foreach (var ticket in invalidAssignments)
        {
            ticket.AssignedAgentId = null;
            ticket.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) return result;

        await transaction.CommitAsync();
        return IdentityResult.Success;
    }

    private async Task<IdentityResult> EnsureAgentRoleAsync()
        => await roles.RoleExistsAsync("Agent") ? IdentityResult.Success : await roles.CreateAsync(new IdentityRole("Agent"));

    private static IdentityResult Validate(object data)
    {
        var errors = new List<ValidationResult>();
        return Validator.TryValidateObject(data, new ValidationContext(data), errors, true)
            ? IdentityResult.Success
            : IdentityResult.Failed(errors.Select(error => new IdentityError
            { Code = "InvalidAccountData", Description = error.ErrorMessage ?? "Popraw dane formularza." }).ToArray());
    }

    private static IdentityResult Failure(string code, string message)
        => IdentityResult.Failed(new IdentityError { Code = code, Description = message });
}
