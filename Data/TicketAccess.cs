using System.Security.Claims;
using layZTickets.DataSets;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

// Read current roles from SQL for each operation, rather than relying on an old cookie.
public sealed record TicketActor(ApplicationUser Account, bool IsAgent, bool IsAdmin)
{
    public bool CanAssign => IsAgent || IsAdmin;
    public bool CanReadInternal => IsAgent || IsAdmin;
    public bool CanReadAll => IsAgent || IsAdmin;
    public bool CanChangeStatus => IsAgent || IsAdmin;

    public bool CanRead(TicketData ticket) => CanReadAll || ticket.CreatedByUserId == Account.Id;
    public bool CanChangePriority(TicketData ticket) => CanRead(ticket);

    public bool CanReply(TicketData ticket) => IsAdmin || (IsAgent
        ? Account.AgentCategory == ticket.Category
        : ticket.CreatedByUserId == Account.Id);
}

public static class TicketAccess
{
    public static IQueryable<TicketData> VisibleTickets(AppDbContext db, TicketActor actor)
        => db.Tickets.Where(ticket => actor.CanReadAll || ticket.CreatedByUserId == actor.Account.Id);

    public static async Task<TicketActor?> GetActorAsync(AppDbContext db, ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principal.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(userId))
            return null;

        var account = await db.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        if (account is null) return null;

        var roles = await (from membership in db.UserRoles
                           join role in db.Roles on membership.RoleId equals role.Id
                           where membership.UserId == userId
                           select role.NormalizedName).ToListAsync(cancellationToken);
        return new TicketActor(account, roles.Contains("AGENT"), roles.Contains("ADMIN"));
    }

    public static IQueryable<ApplicationUser> EligibleAgents(AppDbContext db, TicketCategory category)
        => db.Users.AsNoTracking().Where(account => account.AgentCategory == category &&
            db.UserRoles.Any(membership => membership.UserId == account.Id &&
                db.Roles.Any(role => role.Id == membership.RoleId && role.NormalizedName == "AGENT")));
}
