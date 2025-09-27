using CampusEventHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusEventHub.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var user = _context.Users.Find(userId);
        if (user == null || !user.isAdmin)
        {
            return RedirectToAction("Index", "Home");
        }
        
        return View();
    }
}