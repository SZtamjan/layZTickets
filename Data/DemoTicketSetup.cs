using System.Data;
using layZTickets.DataSets;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

public static class DemoTicketSetup
{
    public static async Task<int> CreateAsync(AppDbContext db)
    {
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Przed dodaniem danych demonstracyjnych zastosuj migracje bazy.");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var staffIds = from membership in db.UserRoles
                       join role in db.Roles on membership.RoleId equals role.Id
                       where role.NormalizedName == "ADMIN" || role.NormalizedName == "AGENT"
                       select membership.UserId;
        var authors = await db.Users.AsNoTracking().Where(account => !staffIds.Contains(account.Id))
            .OrderBy(account => account.Id).Select(account => account.Id).ToListAsync();
        if (authors.Count == 0)
            throw new InvalidOperationException("Utwórz najpierw zwykłe konto użytkownika, które będzie autorem przykładowych zgłoszeń.");

        var existingTitles = (await db.Tickets.Where(ticket => ticket.Title.StartsWith("[DEMO] "))
            .Select(ticket => ticket.Title).ToListAsync()).ToHashSet(StringComparer.Ordinal);
        var now = DateTime.UtcNow;
        var definitions = new[]
        {
            (TicketCategory.Hardware, new[] { "Drukarka nie pobiera papieru", "Laptop wyłącza się podczas pracy",
                "Monitor nie wykrywa sygnału", "Uszkodzona stacja dokująca", "Wymiana dysku w komputerze",
                "Zamówienie klawiatury ergonomicznej", "Naprawa skanera w recepcji", "Instalacja nowej drukarki" }),
            (TicketCategory.Software, new[] { "Brak dostępu do edytora PDF", "Błąd przy uruchamianiu aplikacji księgowej",
                "Aktualizacja pakietu biurowego", "Aplikacja magazynowa nie zapisuje dokumentów", "Odnowienie licencji programu CAD",
                "Konfiguracja podpisu elektronicznego", "Przywrócenie ustawień poczty", "Instalacja narzędzia do wideokonferencji" }),
            (TicketCategory.Network, new[] { "Słaby zasięg Wi-Fi w sali spotkań", "Brak połączenia z serwerem plików",
                "Konfiguracja dostępu VPN", "Przerwy w dostępie do sieci w magazynie", "Oczekiwanie na wymianę przełącznika",
                "Weryfikacja reguł zapory sieciowej", "Przywrócenie dostępu do intranetu", "Podłączenie stanowiska do sieci" }),
            (TicketCategory.Other, new[] { "Prośba o instrukcję obsługi portalu", "Brak dostępu do sali szkoleniowej",
                "Przygotowanie stanowiska dla nowej osoby", "Pilna pomoc przy prezentacji dla klienta", "Uzgodnienie terminu szkolenia",
                "Weryfikacja listy wyposażenia", "Aktualizacja instrukcji dla użytkowników", "Przekazanie sprzętu po szkoleniu" })
        };
        var ages = new[] { 0, 0, 1, 2, 4, 6, 9, 12 };
        var statuses = new[] { TicketStatus.New, TicketStatus.New, TicketStatus.InProgress, TicketStatus.InProgress,
            TicketStatus.Pending, TicketStatus.Pending, TicketStatus.Closed, TicketStatus.Closed };
        var priorities = new[] { TicketPriority.Low, TicketPriority.High, TicketPriority.Normal, TicketPriority.Highest,
            TicketPriority.High, TicketPriority.Normal, TicketPriority.Low, TicketPriority.Normal };
        var added = 0;
        foreach (var (category, titles) in definitions)
        {
            var agentId = await TicketAccess.EligibleAgents(db, category).OrderBy(account => account.Id)
                .Select(account => account.Id).FirstOrDefaultAsync();
            for (var index = 0; index < titles.Length; index++)
            {
                var title = $"[DEMO] {titles[index]}";
                if (existingTitles.Contains(title)) continue;
                var created = now.AddDays(-ages[index]).AddHours(-(index < 2 ? index * 4 + 1 : (int)category + 1));
                var authorId = authors[((int)category + index) % authors.Count];
                var closed = statuses[index] == TicketStatus.Closed ? now.AddDays(index == 6 ? -5 : -2).AddHours(-2) : (DateTime?)null;
                var ticket = new TicketData
                {
                    Title = title,
                    Description = $"Przykładowe zgłoszenie demonstracyjne: {titles[index]}. "
                        + "Proszę o sprawdzenie problemu i informację o dalszych krokach. Dane są fikcyjne i służą prezentacji aplikacji.",
                    Category = category, Priority = priorities[index], Status = statuses[index],
                    CreatedByUserId = authorId, AssignedAgentId = index < 2 ? null : agentId,
                    CreatedAt = created, ClosedAt = closed,
                    UpdatedAt = index < 2 ? null : closed ?? created.AddHours(4)
                };
                if (index >= 2)
                {
                    ticket.Comments.Add(new TicketComment
                    {
                        AuthorUserId = agentId ?? authorId,
                        Content = statuses[index] switch
                        {
                            TicketStatus.Closed => "Problem został rozwiązany. Zgłoszenie zamykamy po potwierdzeniu działania.",
                            TicketStatus.Pending => "Oczekujemy na dodatkowe informacje lub realizację zamówienia.",
                            _ => "Zgłoszenie przyjęte. Trwa sprawdzanie przyczyny problemu."
                        },
                        CreatedAt = closed ?? created.AddHours(3)
                    });
                    if (agentId is not null && index is 2 or 4)
                        ticket.Comments.Add(new TicketComment
                        {
                            AuthorUserId = agentId, IsInternal = true, CreatedAt = created.AddHours(2),
                            Content = "Demonstracyjna notatka wewnętrzna: sprawdzono konfigurację, dalsza diagnoza po stronie zespołu."
                        });
                }
                db.Tickets.Add(ticket);
                added++;
            }
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return added;
    }
}
