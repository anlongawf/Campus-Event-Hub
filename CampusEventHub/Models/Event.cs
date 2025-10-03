using System.ComponentModel.DataAnnotations;

namespace CampusEventHub.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required]
    public string EventName { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public int TrainningPoint { get; set; } = 0;

    [Required]
    public DateTime EventDate { get; set; } = DateTime.Now;
    
    // Danh sách ghế riêng của event này
    public List<Seat> Seats { get; set; } = new();
}