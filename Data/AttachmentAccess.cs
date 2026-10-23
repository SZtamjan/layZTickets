using layZTickets.DataSets;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

public static class AttachmentAccess
{
    public static IQueryable<TicketAttachment> VisibleAttachments(AppDbContext db, TicketActor actor)
    {
        var visibleIds = TicketAccess.VisibleTickets(db, actor).Select(ticket => ticket.Id);
        return db.TicketAttachments.AsNoTracking().Where(file => visibleIds.Contains(file.TicketId)
            && (file.Comment == null || !file.Comment.IsInternal || actor.CanReadInternal));
    }

    public static Task<List<AttachmentInfo>> ListAsync(AppDbContext db, TicketActor actor, int ticketId)
        => VisibleAttachments(db, actor).Where(file => file.TicketId == ticketId)
            .OrderBy(file => file.CreatedAt).ThenBy(file => file.Id)
            .Select(file => new AttachmentInfo(file.Id, file.FileName, file.Size, file.TicketCommentId, file.CreatedAt))
            .ToListAsync();
}
