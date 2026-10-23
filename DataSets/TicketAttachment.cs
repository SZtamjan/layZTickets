namespace layZTickets.DataSets;

public class TicketAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int TicketId { get; set; }
    public TicketData Ticket { get; set; } = default!;
    public int? TicketCommentId { get; set; }
    public TicketComment? Comment { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public layZTickets.Data.ApplicationUser UploadedByUser { get; set; } = default!;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Lista nie pobiera zawartości plików do pamięci komponentu.
public sealed record AttachmentInfo(Guid Id, string FileName, long Size, int? TicketCommentId, DateTime CreatedAt);
