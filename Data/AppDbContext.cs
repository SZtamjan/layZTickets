using layZTickets.Components.Pages;
using layZTickets.DataSets;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TicketData> Tickets => Set<TicketData>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TicketData>()
            .HasOne(ticket => ticket.CreatedByUser)
            .WithMany()
            .HasForeignKey(ticket => ticket.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TicketData>()
            .HasOne(ticket => ticket.AssignedAgent)
            .WithMany()
            .HasForeignKey(ticket => ticket.AssignedAgentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TicketComment>()
            .HasOne(comment => comment.AuthorUser)
            .WithMany()
            .HasForeignKey(comment => comment.AuthorUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TicketComment>()
            .Property(comment => comment.IsInternal)
            .HasDefaultValue(false);

        modelBuilder.Entity<TicketAttachment>(attachment =>
        {
            attachment.HasOne(file => file.Ticket).WithMany(ticket => ticket.Attachments)
                .HasForeignKey(file => file.TicketId).OnDelete(DeleteBehavior.Cascade);
            // Złożony klucz uniemożliwia przypięcie pliku do komentarza z innego zgłoszenia.
            attachment.HasOne(file => file.Comment).WithMany(comment => comment.Attachments)
                .HasForeignKey(file => new { file.TicketCommentId, file.TicketId })
                .HasPrincipalKey(comment => new { comment.Id, comment.TicketId })
                .OnDelete(DeleteBehavior.NoAction);
            attachment.HasOne(file => file.UploadedByUser).WithMany()
                .HasForeignKey(file => file.UploadedByUserId).OnDelete(DeleteBehavior.NoAction);
            attachment.Property(file => file.FileName).HasMaxLength(255).IsRequired();
            attachment.Property(file => file.Content).IsRequired();
            attachment.ToTable(table => table.HasCheckConstraint("CK_TicketAttachments_Size",
                "[Size] >= 0 AND [Size] <= 5242880 AND DATALENGTH([Content]) = [Size]"));
        });
    }
}
