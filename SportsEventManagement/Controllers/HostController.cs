using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SportsEventManagement.Data;
using SportsEventManagement.Models;
using SportsEventManagement.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SportsEventManagement.Controllers
{
    [Route("Host/[action]")]
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HostController(ApplicationDbContext context)
        {
            _context = context;
        }



        private IActionResult VerifyHostAccess()
        {
            var accountType = HttpContext.Session.GetString("AccountType");

            if (string.IsNullOrEmpty(accountType) || accountType != "Host")
            {
                TempData["ErrorMessage"] = "Dostęp tylko dla gospodarzy.";
                return RedirectToAction("MainPage", "Home");
            }

            return null;
        }



        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var result = VerifyHostAccess();
            if (result != null)
            {
                context.Result = result;
            }

            base.OnActionExecuting(context);
        }



        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var activeEvents = _context.Events
                .Where(e => e.HostId == int.Parse(userId) && !e.IsInactive)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Discipline = e.Discipline,
                    Date = e.Date,
                    Location = e.Location,
                    RegistrationStart = e.RegistrationStart,
                    RegistrationEnd = e.RegistrationEnd
                })
                .ToList();

            var inactiveEvents = _context.Events
                .Where(e => e.HostId == int.Parse(userId) && e.IsInactive)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Discipline = e.Discipline,
                    Date = e.Date,
                    Location = e.Location,
                    RegistrationStart = e.RegistrationStart,
                    RegistrationEnd = e.RegistrationEnd
                })
                .ToList();

            var viewModel = new HostIndexViewModel
            {
                ActiveEvents = activeEvents,
                InactiveEvents = inactiveEvents
            };

            return View(viewModel);
        }



        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new EventViewModel());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEvent(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var newEvent = new Event
                {
                    Name = model.Name,
                    Location = model.Location,
                    Date = model.Date,
                    Description = model.Description,
                    RegistrationStart = model.RegistrationStart,
                    RegistrationEnd = model.RegistrationEnd,
                    Capacity = model.Capacity,
                    TeamSize = model.TeamSize,
                    ParticipationType = model.ParticipationType,
                    Discipline = model.Discipline,
                    HostId = int.Parse(userId)
                };

                _context.Events.Add(newEvent);
                _context.SaveChanges();

                TempData["CreateSuccessMessage"] = "Wydarzenie zostało zapisane!";
                return RedirectToAction("Index");
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEvent(int id)
        {
            var eventToDelete = _context.Events
                .Include(e => e.Teams)
                .ThenInclude(t => t.TeamMembers)
                .Include(e => e.EventRegistrations)
                .FirstOrDefault(e => e.Id == id);

            if (eventToDelete == null)
            {
                TempData["ErrorMessage"] = "Wydarzenie nie zostało znalezione.";
                return RedirectToAction("Index");
            }

            foreach (var registration in eventToDelete.EventRegistrations)
            {
                if (registration.UserId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = registration.UserId.Value,
                        Message = $"Wydarzenie \"{eventToDelete.Name}\" zostało odwołane przez gospodarza.",
                        NotificationDate = DateTime.Now
                    };
                    _context.Notifications.Add(notification);
                }
            }

            foreach (var team in eventToDelete.Teams)
            {
                foreach (var member in team.TeamMembers)
                {
                    var notification = new Notification
                    {
                        UserId = member.UserId,
                        Message = $"Wydarzenie \"{eventToDelete.Name}\" zostało odwołane przez gospodarza.",
                        NotificationDate = DateTime.Now
                    };
                    _context.Notifications.Add(notification);
                }
            }

            foreach (var team in eventToDelete.Teams)
            {
                var teamMembers = team.TeamMembers.ToList();
                _context.TeamMembers.RemoveRange(teamMembers);
            }

            _context.Teams.RemoveRange(eventToDelete.Teams);

            _context.Events.Remove(eventToDelete);

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Wydarzenie zostało pomyślnie usunięte. Powiadomienia zostały wysłane do zapisanych uczestników.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas usuwania: {ex.Message}";
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult Details(int id)
        {

            ViewBag.EventId = id;
            return View();
        }



        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby edytować wydarzenie.";
                return RedirectToAction("MainPage", "Home");
            }

            var parsedUserId = int.Parse(userId);

            var eventToEdit = _context.Events.FirstOrDefault(e => e.Id == id && e.HostId == parsedUserId);

            if (eventToEdit == null)
            {
                TempData["ErrorMessage"] = "Nie masz uprawnień do edytowania tego wydarzenia.";
                return RedirectToAction("MainPage", "Home");
            }

            var viewModel = new EventEditViewModel
            {
                Id = eventToEdit.Id,
                Name = eventToEdit.Name,
                Discipline = eventToEdit.Discipline,
                Location = eventToEdit.Location,
                Description = eventToEdit.Description,
                RegistrationStart = eventToEdit.RegistrationStart,
                RegistrationEnd = eventToEdit.RegistrationEnd,
                Date = eventToEdit.Date,
                Capacity = eventToEdit.Capacity,
                TeamSize = eventToEdit.TeamSize,
                ParticipationType = eventToEdit.ParticipationType
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EventEditViewModel model)
        {
            if (!model.IsRegistrationStartEditable)
            {
                ModelState.Remove("ParticipationType");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var eventToUpdate = _context.Events.FirstOrDefault(e => e.Id == model.Id);
            if (eventToUpdate == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
                return RedirectToAction("Index");
            }

            eventToUpdate.Name = model.Name;
            eventToUpdate.Discipline = model.Discipline;
            eventToUpdate.Location = model.Location;
            eventToUpdate.Description = model.Description;
            eventToUpdate.RegistrationStart = model.RegistrationStart;
            eventToUpdate.RegistrationEnd = model.RegistrationEnd;
            eventToUpdate.Date = model.Date;
            eventToUpdate.Capacity = model.Capacity;
            eventToUpdate.ParticipationType = model.ParticipationType;
            eventToUpdate.TeamSize = model.ParticipationType == "Individual" ? 1 : model.TeamSize;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Wydarzenie zostało zaktualizowane.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsInactive(int id)
        {
            var eventToMark = _context.Events.FirstOrDefault(e => e.Id == id);

            if (eventToMark == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
                return RedirectToAction("Index");
            }

            eventToMark.IsInactive = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Wydarzenie zostało oznaczone jako nieobsługiwane.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsUnused(int id)
        {
            var eventToMark = _context.Events.FirstOrDefault(e => e.Id == id);
            if (eventToMark == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
                return RedirectToAction("Index");
            }

            eventToMark.IsInactive = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Wydarzenie zostało oznaczone jako nieużywane.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsActive(int id)
        {
            var eventToMark = _context.Events.FirstOrDefault(e => e.Id == id);
            if (eventToMark == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
                return RedirectToAction("Index");
            }

            eventToMark.IsInactive = false;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Wydarzenie zostało przywrócone do aktywnych.";
            return RedirectToAction("Index");
        }
    }
}
