using layZTickets.Components.Pages;
using layZTickets.DataSets;
using Microsoft.EntityFrameworkCore;

namespace layZTickets.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>()
            .Property(ticket => ticket.TicketNumber)
            .HasMaxLength(16)
            .HasDefaultValue(string.Empty);

        modelBuilder.Entity<Ticket>()
            .HasIndex(ticket => ticket.TicketNumber)
            .IsUnique()
            .HasFilter("[TicketNumber] <> ''");

        modelBuilder.Entity<TicketComment>()
            .Property(comment => comment.IsInternal)
            .HasDefaultValue(false);
    }
}
