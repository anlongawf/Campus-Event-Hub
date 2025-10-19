using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CampusEventHub.Models;
using CampusEventHub.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusEventHub.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    public HomeController(ApplicationDbContext context) 
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            ViewBag.UserName = "Khách";
            ViewBag.Balance = 0m;
            ViewBag.TrainningPoint = 0;
        }
        else
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                ViewBag.UserName = user.UserName ?? "Người dùng";
            }
            else
            {
                ViewBag.UserName = "Khách";
            }
        }

        var events = _context.Events.ToList();
        return View(events);
    }


    public IActionResult Events(int id)
    {
        var ev = _context.Events.FirstOrDefault(e => e.EventId == id);

        if (ev == null)
        {
            return NotFound();
        }

        return View(ev);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Profile()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
            return RedirectToAction("Login", "Auth"); 
        }

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy thông tin người dùng.";
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.Message = user.TrainningPoint >= 100
            ? "🎉 Chúc mừng! Bạn đã đạt đủ 100 điểm rèn luyện."
            : $"📈 Bạn cần thêm {100 - user.TrainningPoint} điểm nữa để đạt 100 điểm.";

        return View(user);
    }
    
    public IActionResult Auth()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}