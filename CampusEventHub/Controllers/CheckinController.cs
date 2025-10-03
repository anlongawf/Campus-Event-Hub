using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEventHub.Models;
using CampusEventHub.Data;

namespace CampusEventHub.Controllers
{
    public class CheckinController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckinController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult CheckinList()
        {
            var checkins = _context.Checkins.Include(c => c.Event).ToList();
            return View(checkins);
        }


        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth"); 

            var checkins = _context.Checkins
                .Include(c => c.Event) 
                .Include(c => c.User) 
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CheckinTime)
                .ToList();

            return View(checkins); 
        }

        [HttpGet("QRCheckin/{qr}")]
        public IActionResult QRCheckin(string qr)
        {
            if (!qr.StartsWith("EV"))
                return View("QRCheckin", null);

            var parts = qr.Substring(2).Split("-U");
            if (parts.Length != 2)
                return View("QRCheckin", null);

            int eventId = int.Parse(parts[0]);
            string userId = parts[1];

            var checkin = _context.Checkins
                .Include(c => c.Event)
                .Include(c => c.User)
                .FirstOrDefault(c => c.EventId == eventId && c.UserId == userId);

            if (checkin == null)
                return View("QRCheckin", null);

            // Kiểm tra thời gian sự kiện
            bool isTooEarly = checkin.Event.EventDate > DateTime.Now;

            // Duyệt nếu hợp lệ và chưa tới giờ
            if (!checkin.IsApproved && !isTooEarly)
            {
                checkin.IsApproved = true;
                checkin.User.TrainningPoint += checkin.PointsAwarded;
                _context.SaveChanges();
            }

            return View("QRCheckin", checkin);
        }

        
        // Admin duyệt check-in và cộng điểm
        [HttpPost]
        public IActionResult Approve(int checkinId)
        {
            var checkin = _context.Checkins
                .Include(c => c.Event)
                .Include(c => c.User)
                .FirstOrDefault(c => c.CheckinId == checkinId);

            if (checkin == null)
                return Json(new { success = false, message = "Check-in không tồn tại" });

            if (checkin.IsApproved)
                return Json(new { success = false, message = "Check-in đã được duyệt" });

            // Cộng điểm cho user
            checkin.IsApproved = true;
            checkin.PointsAwarded = checkin.Event.TrainningPoint;
            checkin.User.TrainningPoint += checkin.PointsAwarded;

            _context.SaveChanges();

            // Nếu muốn, update session nếu admin đang dùng session này
            HttpContext.Session.SetString("TrainningPoint", checkin.User.TrainningPoint.ToString());

            return Json(new { success = true, message = "Duyệt check-in thành công", points = checkin.PointsAwarded });
        }

        // Xem lịch sử check-in của sinh viên
        public IActionResult MyCheckins()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var list = _context.Checkins
                .Include(c => c.Event)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CheckinTime)
                .ToList();

            return View(list);
        }
    }
}
