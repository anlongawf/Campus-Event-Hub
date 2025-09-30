using CampusEventHub.Data;
using CampusEventHub.Models;
using CampusEventHub.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusEventHub.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    
    private readonly SeatService _seatService;

    public AdminController(ApplicationDbContext context, SeatService seatService)
    {
        _context = context;
        _seatService= seatService;
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
        
        return View();
    }

   
    [HttpPost]
    public IActionResult CreateEvent(Event model, string type)
    {
        if (ModelState.IsValid)
        {
            switch (type)
            {
                case "class":
                    model.Seats = _seatService.GenerateSeatsForClass();
                    break;
                case "course":
                    model.Seats = _seatService.GenerateSeatsForCourse();
                    break;
                case "department":
                    model.Seats = _seatService.GenerateSeatsForToanDepartment();
                    break;
                default:
                    model.Seats = _seatService.GenerateSeatsForClass();
                    break;
            }

            _context.Events.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        return View(model);
    }
    
}