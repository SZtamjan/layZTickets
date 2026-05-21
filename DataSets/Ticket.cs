namespace layZTickets.DataSets
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "New";
        public string Priority { get; set; } = "Normal";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public string? AssignedAgentId { get; set; }

        public List<TicketComment> Comments { get; set; } = new();
        //public List<TicketHistory> History { get; set; } = new();
    }
}
