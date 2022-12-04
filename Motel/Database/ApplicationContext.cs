using System.Diagnostics;

#pragma warning disable CS8618

namespace Motel.Database;

public class ApplicationContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<RoomType> RoomTypes { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<LeaseAgreement> LeaseAgreements { get; set; }
    public virtual DbSet<LeaseRoom> LeaseRooms { get; set; }
    public virtual DbSet<RoomCleaning> RoomCleanings { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }
    public virtual DbSet<AdvertisingContract> AdvertisingContracts { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaseRoom>().HasKey(e => new { e.LeaseAgreementId, e.RoomNumber });
        modelBuilder.Entity<RoomCleaning>().HasKey(e => new { e.RoomNumber, e.Datetime });

        /*modelBuilder.Entity<LeaseAgreement>(entity =>
        {
            entity.HasMany(d => d.Rooms).WithMany(p => p.LeaseAgreements)
                .UsingEntity<LeaseRoom>(
                    r => r.HasOne<Room>().WithMany()
                        .HasForeignKey(lr => lr.RoomNumber)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<LeaseAgreement>().WithMany()
                        .HasForeignKey(lr => lr.LeaseAgreementId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j.HasKey(e => new { e.LeaseAgreementId, e.RoomNumber }));
        });*/
    }
}

// dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=motel;Username=postgres;Password=123" --output-dir=Moodels1 Npgsql.EntityFrameworkCore.PostgreSQL