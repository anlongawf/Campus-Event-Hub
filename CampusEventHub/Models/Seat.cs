using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEventHub.Models;

public class Seat
{
    [Key]
    public int SeatId { get; set; }

    public char Row { get; set; }
    public int SeatNumber { get; set; }

    // Trạng thái ghế: 0 = trống, 1 = đã đặt, 2 = Bị Khoa   
    public SeatStatus Status { get; set; } = SeatStatus.Available;

    // FK: ghế thuộc về Event nào
    public int EventId { get; set; }
    [ForeignKey("EventId")]
    public Event Event { get; set; }
}

// Enum cho trạng thái ghế
public enum SeatStatus
{
    Available = 0,
    Booked = 1,
    Reserved = 2
}