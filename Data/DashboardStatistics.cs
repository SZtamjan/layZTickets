using System.Security.Claims;
using layZTickets.DataSets;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

public sealed record DashboardCount(string Label, int Count);
public sealed record DashboardDay(DateTime Date, int Created, int Closed);
public sealed record DashboardAgent(string Name, int OpenCount);
public sealed record DashboardTicket(int Id, string Title, TicketPriority Priority, DateTime CreatedAt);
public sealed record DashboardData(string ViewerName, DateTime RefreshedAt, int Total, int Open,
    int Unassigned, int HighPriority, int ClosedLastWeek, int AssignedToMe,
    List<DashboardCount> Statuses, List<DashboardCount> Categories,
    List<DashboardDay> Days, List<DashboardAgent> Agents, List<DashboardTicket> UrgentTickets);

public static class DashboardStatistics
{
    public static async Task<DashboardData?> LoadAsync(AppDbContext db, ClaimsPrincipal principal,
        DateTime? asOfUtc = null)
    {
        // Cookie może zawierać starą rolę. Statystyki wymagają aktualnych uprawnień z SQL.
        var actor = await TicketAccess.GetActorAsync(db, principal);
        if (actor?.CanReadAll != true) return null;

        var now = (asOfUtc ?? DateTime.UtcNow).ToUniversalTime();
        var start = now.Date.AddDays(-6);
        var query = TicketAccess.VisibleTickets(db, actor).AsNoTracking();
        var totals = await query.GroupBy(ticket => 1).Select(group => new
        {
            Total = group.Count(),
            Open = group.Count(ticket => ticket.Status != TicketStatus.Closed),
            Unassigned = group.Count(ticket => ticket.Status != TicketStatus.Closed && ticket.AssignedAgentId == null),
            HighPriority = group.Count(ticket => ticket.Status != TicketStatus.Closed
                && (ticket.Priority == TicketPriority.High || ticket.Priority == TicketPriority.Highest)),
            ClosedLastWeek = group.Count(ticket => ticket.Status == TicketStatus.Closed
                && ticket.ClosedAt >= start && ticket.ClosedAt <= now),
            AssignedToMe = group.Count(ticket => ticket.Status != TicketStatus.Closed && ticket.AssignedAgentId == actor.Account.Id)
        }).SingleOrDefaultAsync();

        var statusCounts = await query.GroupBy(ticket => ticket.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() }).ToListAsync();
        var categoryCounts = await query.Where(ticket => ticket.Status != TicketStatus.Closed)
            .GroupBy(ticket => ticket.Category)
            .Select(group => new { Category = group.Key, Count = group.Count() }).ToListAsync();
        var createdDays = await query.Where(ticket => ticket.CreatedAt >= start && ticket.CreatedAt <= now)
            .GroupBy(ticket => ticket.CreatedAt.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() }).ToListAsync();
        var closedDays = await query.Where(ticket => ticket.Status == TicketStatus.Closed
                && ticket.ClosedAt >= start && ticket.ClosedAt <= now)
            .GroupBy(ticket => ticket.ClosedAt!.Value.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() }).ToListAsync();
        var assignments = await query.Where(ticket => ticket.Status != TicketStatus.Closed && ticket.AssignedAgentId != null)
            .GroupBy(ticket => new { ticket.AssignedAgentId, ticket.AssignedAgent!.FirstName,
                ticket.AssignedAgent.LastName, ticket.AssignedAgent.UserName })
            .Select(group => new { group.Key.FirstName, group.Key.LastName, group.Key.UserName, Count = group.Count() })
            .OrderByDescending(group => group.Count).ThenBy(group => group.LastName).ThenBy(group => group.FirstName)
            .ThenBy(group => group.UserName).ToListAsync();
        var urgent = await query.Where(ticket => ticket.Status != TicketStatus.Closed
                && (ticket.Priority == TicketPriority.High || ticket.Priority == TicketPriority.Highest))
            .OrderByDescending(ticket => ticket.Priority).ThenBy(ticket => ticket.CreatedAt).ThenBy(ticket => ticket.Id)
            .Take(8).Select(ticket => new DashboardTicket(ticket.Id, ticket.Title, ticket.Priority, ticket.CreatedAt))
            .ToListAsync();

        var statuses = Enum.GetValues<TicketStatus>().Select(status => new DashboardCount(StatusName(status),
            statusCounts.FirstOrDefault(group => group.Status == status)?.Count ?? 0)).ToList();
        var unknownStatuses = statusCounts.Where(group => !Enum.IsDefined(group.Status)).Sum(group => group.Count);
        if (unknownStatuses > 0) statuses.Add(new("Nieznany status", unknownStatuses));
        var categories = Enum.GetValues<TicketCategory>().Select(category => new DashboardCount(CategoryName(category),
            categoryCounts.FirstOrDefault(group => group.Category == category)?.Count ?? 0)).ToList();
        var unknownCategories = categoryCounts.Where(group => !Enum.IsDefined(group.Category)).Sum(group => group.Count);
        if (unknownCategories > 0) categories.Add(new("Nieznana kategoria", unknownCategories));
        var days = Enumerable.Range(0, 7).Select(index => start.AddDays(index)).Select(date => new DashboardDay(date,
            createdDays.FirstOrDefault(day => day.Date == date)?.Count ?? 0,
            closedDays.FirstOrDefault(day => day.Date == date)?.Count ?? 0)).ToList();
        var agents = assignments.Select(group =>
        {
            var name = $"{group.FirstName} {group.LastName}".Trim();
            return new DashboardAgent(string.IsNullOrWhiteSpace(name) ? group.UserName ?? "Nieznane konto" : name, group.Count);
        }).ToList();

        return new DashboardData(actor.Account.DisplayName, now, totals?.Total ?? 0, totals?.Open ?? 0,
            totals?.Unassigned ?? 0, totals?.HighPriority ?? 0, totals?.ClosedLastWeek ?? 0, totals?.AssignedToMe ?? 0,
            statuses, categories, days, agents, urgent);
    }

    private static string StatusName(TicketStatus status) => status switch
    {
        TicketStatus.New => "Nowe", TicketStatus.InProgress => "W trakcie",
        TicketStatus.Pending => "Oczekujące", TicketStatus.Closed => "Zamknięte", _ => "Nieznany status"
    };

    private static string CategoryName(TicketCategory category) => category switch
    {
        TicketCategory.Hardware => "Sprzęt", TicketCategory.Software => "Oprogramowanie",
        TicketCategory.Network => "Sieć", TicketCategory.Other => "Inne", _ => "Nieznana kategoria"
    };
}
