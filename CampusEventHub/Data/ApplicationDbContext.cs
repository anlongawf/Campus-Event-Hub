using CampusEventHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEventHub.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Checkin> Checkins { get; set; }
    public DbSet<PaymentOrder> PaymentOrders { get; set; } 
}