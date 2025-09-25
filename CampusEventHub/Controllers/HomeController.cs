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
        var userId= HttpContext.Session.GetString("UserId");
        string username = "Khách";

        if (userId == null)
        {
            ViewBag.UserName = "Khách";
        }
        else
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                username = user.UserName ?? "Người dùng";
            }
            ViewBag.UserName = username;
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