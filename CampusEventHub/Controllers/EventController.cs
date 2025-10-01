using CampusEventHub.Data;
using CampusEventHub.Models;
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

    // Xử lý đặt ghế
    [HttpPost]
    public IActionResult BookSeat(int eventId, int seatId)
    {
        var seat = _context.Seats
            .FirstOrDefault(s => s.SeatId == seatId && s.EventId == eventId);

        if (seat == null)
        {
            return Json(new { success = false, message = "Ghế không tồn tại" });
        }
        
        // Xử lí kiểm tra nếu có yêu cầu thanh toán
        //
        //
        

        if (seat.Status != SeatStatus.Available)
        {
            return Json(new { success = false, message = "Ghế đã được đặt hoặc bị khóa" });
        }

        seat.Status = SeatStatus.Booked;
        _context.SaveChanges();

        return Json(new { success = true, message = "Đặt ghế thành công" });
    }
    
}