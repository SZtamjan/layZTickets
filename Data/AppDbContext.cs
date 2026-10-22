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
    }
}
