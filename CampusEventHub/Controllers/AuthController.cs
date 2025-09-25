using Microsoft.AspNetCore.Mvc;
using CampusEventHub.Models;
using CampusEventHub.Data;
using CampusEventHub.Service;
using CampusEventHub.DTO;

namespace CampusEventHub.Controllers;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;
    
    private readonly MailService _mailService;

    
    public AuthController(ApplicationDbContext context, MailService mailService)
    {
        _context = context;
        _mailService = mailService;
    }

    public ActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(RegisterDTO model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu không hợp lệ.";
            return View(model);
        }
        
        var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);


        if (user != null && user.Password == model.Password)
        {
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            ViewBag.UserName = user.UserName;
            TempData["Success"] = "Đăng nhập thành công!";
            var checkUserId = HttpContext.Session.GetString("UserId");
            Console.WriteLine("UserId after set: " + checkUserId);
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // Response.WriteAsync("<script>alert('Registration Failed')</script>");
            TempData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View(model);
        }
    }

    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Register(User user)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Error = "Dữ liệu không hợp lệ";
            return View(user);
        }
        
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ViewBag.Message = "User already exists";
            return View(user); 
        }
        
        string verificationCode = _mailService.GenerateVerificationCode();
        
        user.IsVerified = false;
        
        _context.Users.Add(user);
        _context.SaveChanges();
        
        TempData["VerificationCode"] = verificationCode;
        TempData["UserEmail"] = user.Email;
        
        _mailService.SendVerificationEmail(user.Email, verificationCode);
        
        TempData["Success"] = "Verification email sent.";
        return RedirectToAction("VerifyCode");
    }

    [HttpGet]
    public ActionResult VerifyCode()
    {
        return View();
    }

    [HttpPost]
    public ActionResult VerifyCode(VerifyEmailModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        string ?savedCode= TempData["VerificationCode"] as string;
        string ?userEmail = TempData["UserEmail"] as string;
        
        if (savedCode == null || userEmail == null)
        {
            TempData["Error"] = "Session hết hạn, vui lòng đăng ký lại.";
            return RedirectToAction("Register");
        }

        if (model.Code == savedCode)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user != null)
            {
                user.IsVerified = true;
                _context.SaveChanges();
                TempData["Success"] = "Email verified successfully!";
                return RedirectToAction("Login");
            }
        }
        else
        {
            TempData["Error"] = "Mã xác minh không đúng!";
            return View(model);
        }
        return View(model);
    }
    
}