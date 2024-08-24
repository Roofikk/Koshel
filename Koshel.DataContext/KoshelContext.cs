using Microsoft.EntityFrameworkCore;

namespace Koshel.DataContext;

public class KoshelContext : DbContext
{
    public DbSet<Message> Messages { get; set; }

    public KoshelContext()
        :base()
    {
    }

    public KoshelContext(DbContextOptions<KoshelContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=koshel;Username=postgres;Password=RufikRoot123321");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(m => m.MessageId);
            e.Property(m => m.MessageId).ValueGeneratedOnAdd();
        });
    }
}
