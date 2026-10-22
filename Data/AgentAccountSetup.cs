using layZTickets.DataSets;
using Microsoft.AspNetCore.Identity;

namespace layZTickets.Data;

public static class AgentAccountSetup
{
    public static async Task CreateAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var accounts = new[]
        {
            (TicketCategory.Hardware, "Adam", "Nowak"),
            (TicketCategory.Software, "Ewa", "Kowalska"),
            (TicketCategory.Network, "Piotr", "Wiśniewski"),
            (TicketCategory.Other, "Anna", "Zielińska")
        };

        // Validate configuration before writing anything. Passwords never live in the repository.
        foreach (var (category, _, _) in accounts)
        {
            if (string.IsNullOrWhiteSpace(configuration[$"BootstrapAgents:{category}:Password"]))
                throw new InvalidOperationException($"Brakuje hasła BootstrapAgents:{category}:Password.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        if (!await roles.RoleExistsAsync("Agent"))
            EnsureSuccess(await roles.CreateAsync(new IdentityRole("Agent")));

        foreach (var (category, firstName, lastName) in accounts)
        {
            var email = $"agent.{category.ToString().ToLowerInvariant()}@layztickets.test";
            var account = await users.FindByEmailAsync(email);
            if (account is null)
            {
                account = new ApplicationUser
                {
                    Email = email, UserName = email, FirstName = firstName, LastName = lastName,
                    AgentCategory = category
                };
                EnsureSuccess(await users.CreateAsync(account, configuration[$"BootstrapAgents:{category}:Password"]!));
                EnsureSuccess(await users.AddToRoleAsync(account, "Agent"));
            }
            else if (account.AgentCategory != category || !await users.IsInRoleAsync(account, "Agent"))
            {
                // Do not elevate or repurpose an unrelated account on an email collision.
                throw new InvalidOperationException($"Istniejące konto {email} nie jest oczekiwanym agentem.");
            }
        }

        await transaction.CommitAsync();
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
