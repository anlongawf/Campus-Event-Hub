using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEventHub.Models;

public class Checkin
{
    [Key]
    public int CheckinId { get; set; }
    
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    public int EventId { get; set; }
    [ForeignKey("EventId")]
    public Event Event { get; set; }
    
    // Trạng thái xét duyệt điểm
    public bool IsApproved { get; set; } = false;

    // Thời gian check-in
    public DateTime CheckinTime { get; set; } = DateTime.Now;

    // Điểm rèn luyện được nhận (sẽ bằng Event.TrainningPoint khi duyệt)
    public int PointsAwarded { get; set; } = 0;
    
    public string QRCode { get; set; }
    
}