using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsEventManagement.Data;
using SportsEventManagement.Models;
using System.Linq;

namespace SportsEventManagement.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby zobaczyć powiadomienia.";
                return RedirectToAction("Login", "Account");
            }

            int parsedUserId = int.Parse(userId);

            var notifications = _context.Notifications
                .Where(n => n.UserId == parsedUserId)
                .OrderByDescending(n => n.NotificationDate)
                .ToList();

            return View(notifications);
        }



        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }



        [HttpGet]
        public JsonResult GetUnreadCount()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { unreadCount = 0 });
            }

            int userId = int.Parse(userIdString);
            var unreadCount = _context.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);

            return Json(new { unreadCount });


        }


        public void AddNotification(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now,
                NotificationDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNotification(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby usunąć powiadomienie.";
                return RedirectToAction("Index", "Home");
            }

            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id && n.UserId == int.Parse(userId));
            if (notification == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono powiadomienia lub nie masz uprawnień do jego usunięcia.";
                return RedirectToAction("Index");
            }

            _context.Notifications.Remove(notification);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Powiadomienie zostało usunięte.";
            return RedirectToAction("Index");
        }
    }
}
