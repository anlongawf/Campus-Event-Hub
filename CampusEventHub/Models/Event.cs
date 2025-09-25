using System.ComponentModel.DataAnnotations;

namespace CampusEventHub.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Location { get; set; }
    public int TrainningPoint { get; set; } = 0;
    public DateTime EventDate { get; set; } = DateTime.Now;
    
    public int SeatId { get; set; }
    
    public int Capacity { get; set; }
    
    public Seat Seat { get; set; }
}