using CampusEventHub.Data;
using CampusEventHub.Models;
using CampusEventHub.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusEventHub.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SeatService _seatService;

    public AdminController(ApplicationDbContext context, SeatService seatService)
    {
        _context = context;
        _seatService = seatService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // var userId = HttpContext.Session.GetString("UserId");
        // var user = _context.Users.Find(userId);
        // if (user == null || !user.isAdmin)
        // {
        //     return RedirectToAction("Index", "Home");
        // }
        
        var events = _context.Events
            .Include(e => e.Seats)
            .OrderByDescending(e => e.EventDate)
            .ToList();
        
        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new Event());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateEvent(Event model, string type)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Tạo ghế dựa vào loại phạm vi
                model.Seats = type switch
                {
                    "class" => _seatService.GenerateSeatsForClass(),
                    "course" => _seatService.GenerateSeatsForCourse(),
                    "department" => _seatService.GenerateSeatsForToanDepartment(),
                    _ => _seatService.GenerateSeatsForClass()
                };

                _context.Events.Add(model);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Tạo sự kiện thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tạo sự kiện: {ex.Message}");
            }
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult EditEvent(int id)
    {
        var eventItem = _context.Events
            .Include(e => e.Seats)
            .FirstOrDefault(e => e.EventId == id);
        
        if (eventItem == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sự kiện!";
            return RedirectToAction("Index");
        }
        
        return View(eventItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditEvent(Event model)
    {
        // Remove validation cho Seats vì không cần validate
        ModelState.Remove("Seats");
        
        if (ModelState.IsValid)
        {
            try
            {
                var existing = _context.Events
                    .Include(e => e.Seats)
                    .FirstOrDefault(e => e.EventId == model.EventId);
                
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sự kiện!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra xem có ghế nào đã được đặt không
                // Uncomment nếu Seat model có IsBooked property
                // bool hasBookedSeats = existing.Seats.Any(s => s.IsBooked);
                // if (hasBookedSeats && existing.EventDate != model.EventDate)
                // {
                //     TempData["WarningMessage"] = "Cảnh báo: Sự kiện đã có người đặt ghế!";
                // }

                // Cập nhật các thông tin
                existing.EventName = model.EventName;
                existing.Description = model.Description;
                existing.Price = model.Price;
                existing.ImageUrl = model.ImageUrl;
                existing.Location = model.Location;
                existing.EventDate = model.EventDate;
                existing.TrainningPoint = model.TrainningPoint;

                // KHÔNG cập nhật Seats để giữ nguyên trạng thái đặt ghế
                // existing.Seats = model.Seats; 

                _context.SaveChanges();
                
                TempData["SuccessMessage"] = "Cập nhật sự kiện thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");
            }
        }

        // Nếu validation fail, load lại Seats từ database
        var eventWithSeats = _context.Events
            .Include(e => e.Seats)
            .FirstOrDefault(e => e.EventId == model.EventId);
        
        if (eventWithSeats != null)
        {
            model.Seats = eventWithSeats.Seats;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteEvent(int id)
    { 
        try
        {
            var eventItem = _context.Events
                .Include(e => e.Seats)
                .FirstOrDefault(e => e.EventId == id);
            
            if (eventItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sự kiện!";
                return RedirectToAction("Index");
            }

            // Kiểm tra xem có ghế nào đã được đặt không
            // Uncomment nếu Seat model có IsBooked property
            // bool hasBookedSeats = eventItem.Seats.Any(s => s.IsBooked);
            // if (hasBookedSeats)
            // {
            //     TempData["ErrorMessage"] = "Không thể xóa sự kiện đã có người đặt ghế!";
            //     return RedirectToAction("Index");
            // }

            _context.Events.Remove(eventItem);
            _context.SaveChanges();
            
            TempData["SuccessMessage"] = "Xóa sự kiện thành công!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult ViewEventDetails(int id)
    {
        var eventItem = _context.Events
            .Include(e => e.Seats)
            .FirstOrDefault(e => e.EventId == id);
        if (eventItem == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sự kiện!";
            return RedirectToAction("Index");
        }
        
        return View(eventItem);
    }
}