using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace layZTickets.Data
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public layZTickets.DataSets.TicketCategory? AgentCategory { get; set; }

        [NotMapped]
        public string DisplayName => string.IsNullOrWhiteSpace($"{FirstName} {LastName}".Trim())
            ? UserName ?? "Użytkownik"
            : $"{FirstName} {LastName}".Trim();
    }
}
