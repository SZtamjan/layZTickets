using System.ComponentModel.DataAnnotations.Schema;

namespace layZTickets.DataSets
{
    public class TicketData
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketCategory Category { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.New;
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public layZTickets.Data.ApplicationUser CreatedByUser { get; set; } = default!;
        public string? AssignedAgentId { get; set; }
        public layZTickets.Data.ApplicationUser? AssignedAgent { get; set; }

        public List<TicketComment> Comments { get; set; } = new();
        public List<TicketAttachment> Attachments { get; set; } = new();

        [NotMapped]
        public string TicketNumber => $"IT-{Id}";
    }
}
