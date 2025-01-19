using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsEventManagement.Data;
using SportsEventManagement.Models;
using System.Linq;

namespace SportsEventManagement.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult CurrentEvents()
        {
            var events = _context.Events
                .Where(e => e.Date >= DateTime.Now)
                .Include(e => e.Discipline)
                .Select(e => new
                {
                    e.Name,
                    e.Location,
                    e.Date,
                    e.RegistrationStart,
                    e.RegistrationEnd,
                    e.Discipline,
                    RegistrationStatus = GetRegistrationStatus(e.RegistrationStart, e.RegistrationEnd)
                })
                .ToList();

            return View(events);
        }



        [HttpGet]
        public IActionResult ArchivedEvents()
        {
            var events = _context.Events
                .Where(e => e.Date < DateTime.Now)
                .Include(e => e.Discipline)
                .Select(e => new
                {
                    e.Name,
                    e.Location,
                    e.Date,
                    e.RegistrationStart,
                    e.RegistrationEnd,
                    e.Discipline,
                    RegistrationStatus = GetRegistrationStatus(e.RegistrationStart, e.RegistrationEnd)
                })
                .ToList();

            return View(events);
        }



        private string GetRegistrationStatus(DateTime? registrationStart, DateTime? registrationEnd)
        {
            var now = DateTime.Now;

            if (registrationStart.HasValue && registrationEnd.HasValue)
            {
                if (now < registrationStart.Value)
                {
                    var timeRemaining = registrationStart.Value - now;
                    return $"Start rejestracji za {timeRemaining.Days} dni i {timeRemaining.Hours} godzin";
                }
                else if (now >= registrationStart.Value && now <= registrationEnd.Value)
                {
                    var timeRemaining = registrationEnd.Value - now;
                    return $"Koniec rejestracji za {timeRemaining.Days} dni i {timeRemaining.Hours} godzin";
                }
                else
                {
                    return $"Koniec rejestracji: {registrationStart.Value:dd.MM.yyyy HH:mm} - {registrationEnd.Value:dd.MM.yyyy HH:mm}";
                }
            }

            return "Brak informacji o rejestracji";
        }

    }
}
