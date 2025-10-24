using CampusEventHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEventHub.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Checkin> Checkins { get; set; }
    public DbSet<PaymentOrder> PaymentOrders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnType("varchar(36)");
                
            entity.Property(e => e.UserName)
                .HasColumnType("varchar(100)");
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasColumnType("varchar(255)");
                
            entity.Property(e => e.Password)
                .HasColumnType("varchar(255)");
                
            entity.Property(e => e.TrainningPoint)
                .HasDefaultValue(0);
                
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);
                
            entity.Property(e => e.isAdmin)
                .HasDefaultValue(false);
                
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false);
                
            entity.Property(e => e.RegistrationDate)
                .HasColumnType("datetime");
        });
        
        // Configure Event entity
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            
            entity.Property(e => e.EventName)
                .IsRequired()
                .HasColumnType("varchar(255)");
                
            entity.Property(e => e.Description)
                .HasColumnType("text");
                
            entity.Property(e => e.ImageUrl)
                .HasColumnType("varchar(500)");
                
            entity.Property(e => e.Location)
                .HasColumnType("varchar(255)");
                
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.TrainningPoint)
                .HasDefaultValue(0);
                
            entity.Property(e => e.EventDate)
                .IsRequired()
                .HasColumnType("datetime");
                
            // One-to-many relationship with Seats
            entity.HasMany(e => e.Seats)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure Seat entity
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId);
            
            entity.Property(e => e.Row)
                .IsRequired()
                .HasColumnType("char(1)");
                
            entity.Property(e => e.SeatNumber)
                .IsRequired();
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(SeatStatus.Available);
                
            entity.Property(e => e.UserId)
                .HasColumnType("varchar(36)");
                
            // Foreign key to Event
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Seats)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Foreign key to User (optional)
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configure Checkin entity
        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => e.CheckinId);
            
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnType("varchar(36)");
                
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false);
                
            entity.Property(e => e.CheckinTime)
                .HasColumnType("datetime");
                
            entity.Property(e => e.PointsAwarded)
                .HasDefaultValue(0);
                
            entity.Property(e => e.QRCode)
                .HasColumnType("varchar(255)");
                
            // Foreign key to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Foreign key to Event
            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure PaymentOrder entity
        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.OrderCode)
                .IsRequired();
                
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnType("varchar(36)");
                
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasColumnType("varchar(50)");
                
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");
                
            // Foreign key to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}