using System.ComponentModel.DataAnnotations;

namespace CampusEventHub.Models;

public class Seat
{
    [Key]
    public int SeatId { get; set; }
    public char Row { get; set; }
    public int SeatNumber { get; set; }
    
    // Một Seat có thể được dùng cho nhiều Event
    public ICollection<Event> Events { get; set; }
}