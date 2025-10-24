using CampusEventHub.Data;
using CampusEventHub.Models;
using CampusEventHub.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusEventHub.Controllers;

public class EventController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hiển thị danh sách sự kiện
    [HttpGet]
    public IActionResult Index()
    {
        var events = _context.Events.ToList();
        return View(events);
    }
    
    // Hiển thị danh sách sự kiện chi tiết
    public IActionResult Details(int id)
    {
        var evt = _context.Events
            .Include(e => e.Seats)
            .FirstOrDefault(e => e.EventId == id);

        if (evt == null)
            return NotFound();

        return View(evt); // trả về EventDetails.cshtml
    }

    // Hiển thị giao diện chọn ghế
    [HttpGet]
    public IActionResult SelectSeat(int id)
    {
        var evt = _context.Events
            .Include(e => e.Seats)
            .FirstOrDefault(e => e.EventId == id);

        if (evt == null)
        {
            return NotFound();
        }

        return View(evt);
    }

    [HttpPost]
    public IActionResult BookSeat(int eventId, int seatId)
    {
        var seat = _context.Seats
            .Include(s => s.Event) // cần load Event để lấy giá
            .FirstOrDefault(s => s.SeatId == seatId && s.EventId == eventId);
    
        if (seat == null)
            return Json(new { success = false, message = "Ghế không tồn tại" });

        var userId = HttpContext.Session.GetString("UserId"); 

        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "Người dùng chưa đăng nhập" });

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return Json(new { success = false, message = "Người dùng không tồn tại" });

        var price = seat.Event?.Price ?? 0;

        if (seat.Status != SeatStatus.Available)
            return Json(new { success = false, message = "Ghế đã được đặt hoặc bị khóa" });

        if (user.Balance < price)
            return Json(new { success = false, message = "Số dư không đủ" });

        // Trừ tiền user
        user.Balance -= price;

        // Set trạng thái ghế
        seat.UserId = user.UserId;
        seat.Status = SeatStatus.Booked;
        
        string qrUrl = $"http://connect.mineclubvn.com:3001//QRCheckin/EV{seat.EventId}-U{user.UserId}";
        string qrCodeBase64 = QRCodeHelper.GenerateQRCodeBase64(qrUrl);

        
        var checkin = new Checkin
        {
            UserId = user.UserId,
            EventId = seat.EventId,
            IsApproved = false,
            PointsAwarded = seat.Event.TrainningPoint,
            QRCode = qrCodeBase64,
            CheckinTime = DateTime.Now
        };
        
        _context.Checkins.Add(checkin);

        _context.SaveChanges();
        HttpContext.Session.SetString("Balance", user.Balance.ToString());
        HttpContext.Session.SetString("TrainningPoint", user.TrainningPoint.ToString());

        return Json(new { success = true, message = "Đặt ghế thành công", remainingBalance = user.Balance });
    }

    
}