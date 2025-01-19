using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsEventManagement.Data;
using SportsEventManagement.Models;
using SportsEventManagement.ViewModels;

namespace SportsEventManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly NotificationsController _notificationsController;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }



        [HttpGet]
        public IActionResult Index(string disciplineFilter)
        {
            var events = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(disciplineFilter))
            {
                events = events.Where(e => e.Discipline.Contains(disciplineFilter));

                ViewBag.FilterText = disciplineFilter;
            }
            else
            {
                ViewBag.FilterText = null;
            }

            return RedirectToAction("CurrentEvents", new { disciplineFilter });
        }



        [HttpGet]
        public IActionResult CurrentEvents(string disciplineFilter)
        {
            ViewData["ActiveTab"] = "CurrentEvents";

            var today = DateTime.Now;
            var currentEvents = _context.Events
                .Where(e => e.Date >= today)
                .AsQueryable();

            if (!string.IsNullOrEmpty(disciplineFilter))
            {
                currentEvents = currentEvents
                    .Where(e => e.Discipline.Contains(disciplineFilter));
            }

            var eventViewModels = currentEvents
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Location = e.Location,
                    Discipline = e.Discipline,
                    RegistrationStart = e.RegistrationStart,
                    RegistrationEnd = e.RegistrationEnd,
                }).ToList();

            ViewBag.DisciplineFilter = disciplineFilter;
            return View("Index", eventViewModels.OrderBy(e => e.Date).ToList());
        }



        [HttpGet]
        public IActionResult ArchivedEvents(string disciplineFilter)
        {
            ViewData["ActiveTab"] = "ArchivedEvents";

            var today = DateTime.Now;
            var archivedEvents = _context.Events
                .Where(e => e.Date < today)
                .AsQueryable();

            if (!string.IsNullOrEmpty(disciplineFilter))
            {
                archivedEvents = archivedEvents
                    .Where(e => e.Discipline.Contains(disciplineFilter));
            }

            var eventViewModels = archivedEvents.Select(e => new EventViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Date = e.Date,
                Location = e.Location,
                Discipline = e.Discipline,
                RegistrationStart = e.RegistrationStart,
                RegistrationEnd = e.RegistrationEnd,
            }).ToList();

            ViewBag.DisciplineFilter = disciplineFilter;
            return View("Index", eventViewModels.OrderByDescending(e => e.Date).ToList());
        }


        public IActionResult Privacy()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public IActionResult MainPage()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Details(int id)
        {
            var eventDetails = _context.Events
                .Where(e => e.Id == id)
                .Select(e => new EventDetailsViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Location = e.Location,
                    RegistrationStart = e.RegistrationStart,
                    RegistrationEnd = e.RegistrationEnd,
                    ContactEmail = e.Host.Email,
                    ContactPhone = e.Host.PhoneNumber,
                    Description = e.Description,
                    Discipline = e.Discipline,
                    RegisteredParticipants = e.OccupiedSeats,
                    MaxParticipants = e.Capacity,
                    CompetitionType = e.ParticipationType == "Team"
                        ? $"Dru¿ynowe ({e.TeamSize}-osobowe dru¿yny)"
                        : "Indywidualnie"
                })
                .FirstOrDefault();

            if (eventDetails == null)
            {
                return NotFound();
            }

            var eventType = _context.Events.Where(e => e.Id == id).Select(e => e.ParticipationType).FirstOrDefault();
            if (eventType == "Team")
            {
                TempData["ErrorMessage"] = "Nie mo¿na wyœwietliæ szczegó³ów tego wydarzenia.";
                return RedirectToAction("MainPage", "Home");
            }

            return View(eventDetails);
        }



        [HttpGet]
        public IActionResult DetailsTeam(int id)
        {
            var eventDetails = _context.Events
                .Where(e => e.Id == id)
                .Select(e => new EventDetailsViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Location = e.Location,
                    RegistrationStart = e.RegistrationStart,
                    RegistrationEnd = e.RegistrationEnd,
                    ContactEmail = e.Host.Email,
                    ContactPhone = e.Host.PhoneNumber,
                    Description = e.Description,
                    Discipline = e.Discipline,
                    RegisteredParticipants = e.OccupiedSeats,
                    MaxParticipants = e.Capacity,
                    CompetitionType = e.ParticipationType == "Team"
                        ? $"Dru¿ynowe ({e.TeamSize}-osobowe dru¿yny)"
                        : "Indywidualnie"
                })
                .FirstOrDefault();

            if (eventDetails == null)
            {
                return NotFound();
            }

            var eventType = _context.Events.Where(e => e.Id == id).Select(e => e.ParticipationType).FirstOrDefault();
            if (eventType != "Team")
            {
                TempData["ErrorMessage"] = "Nie mo¿na wyœwietliæ szczegó³ów tego wydarzenia.";
                return RedirectToAction("MainPage", "Home");
            }

            return View(eventDetails);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterForEvent(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz byæ zalogowany, aby zapisaæ siê na wydarzenie.";
                return RedirectToAction("Details", new { id = eventId });
            }

            var eventDetails = _context.Events.FirstOrDefault(e => e.Id == eventId);

            if (eventDetails == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
                return RedirectToAction("Details", new { id = eventId });
            }


            if (DateTime.Now < eventDetails.RegistrationStart || DateTime.Now > eventDetails.RegistrationEnd)
            {
                TempData["ErrorMessage"] = "Nie mo¿na zapisaæ siê na wydarzenie poza terminem rejestracji.";
                return RedirectToAction("Details", new { id = eventId });
            }


            var isAlreadyRegistered = _context.EventRegistrations.Any(r => r.EventId == eventId && r.UserId == int.Parse(userId));
            if (isAlreadyRegistered)
            {
                TempData["ErrorMessage"] = "Jesteœ ju¿ zapisany na to wydarzenie.";
                return RedirectToAction("Details", new { id = eventId });
            }

            if (eventDetails.OccupiedSeats >= eventDetails.Capacity)
            {
                TempData["ErrorMessage"] = "Brak dostêpnych miejsc na to wydarzenie.";
                return RedirectToAction("Details", new { id = eventId });
            }

            var registration = new EventRegistration
            {
                EventId = eventId,
                UserId = int.Parse(userId),
                RegistrationDate = DateTime.Now
            };

            _context.EventRegistrations.Add(registration);

            eventDetails.OccupiedSeats++;
            _context.SaveChanges();

            _context.Notifications.Add(new Notification
            {
                UserId = int.Parse(userId),
                Message = $"Zosta³eœ zapisany na wydarzenie {eventDetails.Name}.",
                NotificationDate = DateTime.Now
            });
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Zosta³eœ pomyœlnie zapisany na wydarzenie!";
            return RedirectToAction("Details", new { id = eventId });
        }



        private IActionResult RedirectToEventDetails(int eventId)
        {
            var eventDetails = _context.Events.FirstOrDefault(e => e.Id == eventId);

            if (eventDetails != null && eventDetails.ParticipationType == "Team")
            {
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }
            return RedirectToAction("Details", new { id = eventId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterTeamForEvent(int eventId, string teamName)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz byæ zalogowany, aby zapisaæ dru¿ynê.";
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }

            int parsedUserId = int.Parse(userId);

            var eventDetails = _context.Events.Include(e => e.Teams).FirstOrDefault(e => e.Id == eventId);

            if (eventDetails == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }

            if (DateTime.Now < eventDetails.RegistrationStart || DateTime.Now > eventDetails.RegistrationEnd)
            {
                TempData["ErrorMessage"] = "Nie mo¿na zarejestrowaæ dru¿yny poza terminem rejestracji.";
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }

            if (_context.TeamMembers.Any(tm => tm.UserId == parsedUserId && tm.Team.EventId == eventId))
            {
                TempData["ErrorMessage"] = "Ju¿ jesteœ zapisany na to wydarzenie w ramach dru¿yny.";
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }

            var isTeamNameTaken = _context.Teams.Any(t => t.TeamName == teamName && t.EventId == eventId);
            if (isTeamNameTaken)
            {
                TempData["ErrorMessage"] = "Dru¿yna o takiej nazwie ju¿ istnieje.";
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }

            if (eventDetails.OccupiedSeats >= eventDetails.Capacity)
            {
                TempData["ErrorMessage"] = "Brak dostêpnych miejsc na to wydarzenie.";
                return RedirectToAction("DetailsTeam", new { id = eventId });
            }

            var newTeam = new Team
            {
                TeamName = teamName,
                EventId = eventId,
                Capacity = eventDetails.TeamSize ?? 0,
                CaptainId = parsedUserId
            };

            _context.Teams.Add(newTeam);
            _context.SaveChanges();

            var teamMember = new TeamMember
            {
                TeamId = newTeam.Id,
                UserId = parsedUserId
            };
            _context.TeamMembers.Add(teamMember);

            eventDetails.OccupiedSeats++;
            _context.SaveChanges();

            _context.Notifications.Add(new Notification
            {
                UserId = parsedUserId,
                Message = $"Dru¿yna \"{teamName}\" zosta³a pomyœlnie zarejestrowana na wydarzenie \"{eventDetails.Name}\".",
                NotificationDate = DateTime.Now
            });
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Dru¿yna zosta³a pomyœlnie za³o¿ona i zarejestrowana na wydarzenie!";
            return RedirectToAction("DetailsTeam", new { id = eventId });
        }



        public IActionResult Participants(int eventId)
        {
            var eventDetails = _context.Events
                .Include(e => e.EventRegistrations)
                .ThenInclude(r => r.User)
                .FirstOrDefault(e => e.Id == eventId);

            if (eventDetails == null)
            {
                return NotFound("Nie znaleziono wydarzenia.");
            }

            var participants = eventDetails.EventRegistrations
                .Where(r => r.UserId.HasValue)
                .Select(r => new ParticipantViewModel
                {
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName
                })
                .ToList();

            ViewBag.EventName = eventDetails.Name;

            return View(participants);
        }



        [HttpGet]
        public IActionResult DetailsRedirect(int id)
        {
            var eventDetails = _context.Events.FirstOrDefault(e => e.Id == id);

            if (eventDetails == null)
            {
                return NotFound("Nie znaleziono wydarzenia.");
            }

            if (eventDetails.ParticipationType == "Team")
            {
                return RedirectToAction("DetailsTeam", new { id });
            }

            return RedirectToAction("Details", new { id });
        }



        public IActionResult TeamParticipants(int eventId)
        {
            var teams = _context.Teams
                .Where(t => t.EventId == eventId)
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .Select(t => new TeamParticipantViewModel
                {
                    TeamName = t.TeamName,
                    Members = t.TeamMembers.Select(tm => new MemberViewModel
                    {
                        Name = tm.User.FirstName + " " + tm.User.LastName,
                        IsCaptain = tm.UserId == t.CaptainId
                    }).ToList()
                }).ToList();

            ViewBag.EventName = _context.Events.FirstOrDefault(e => e.Id == eventId)?.Name;
            return View(teams);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvitation(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz byæ zalogowany, aby zaakceptowaæ zaproszenie.";
                return RedirectToAction("TeamInvitations");
            }

            var invitation = _context.TeamInvitations
                .Include(ti => ti.Team)
                .ThenInclude(t => t.Captain)
                .FirstOrDefault(ti => ti.Id == id);

            if (invitation == null || invitation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Nie znaleziono zaproszenia.";
                return RedirectToAction("TeamInvitations");
            }

            if (_context.TeamMembers.Count(tm => tm.TeamId == invitation.TeamId) >= invitation.Team.Capacity)
            {
                TempData["ErrorMessage"] = "Dru¿yna osi¹gnê³a maksymaln¹ liczbê cz³onków.";
                return RedirectToAction("TeamInvitations");
            }

            invitation.Status = "Accepted";

            var newMember = new TeamMember
            {
                TeamId = invitation.TeamId,
                UserId = invitation.InvitedUserId
            };
            _context.TeamMembers.Add(newMember);

            var invitedUser = _context.Users.FirstOrDefault(u => u.Id == invitation.InvitedUserId);
            if (invitedUser != null)
            {
                var notification = new Notification
                {
                    UserId = invitation.Team.CaptainId,
                    Message = $"{invitedUser.FirstName} {invitedUser.LastName} do³¹czy³ do Twojej dru¿yny {invitation.Team.TeamName}.",
                    NotificationDate = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Do³¹czy³eœ do dru¿yny.";
            return RedirectToAction("TeamInvitations");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectInvitation(int id)
        {
            var invitation = _context.TeamInvitations.FirstOrDefault(ti => ti.Id == id);

            if (invitation == null || invitation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Nie znaleziono zaproszenia.";
                return RedirectToAction("Invitations");
            }

            invitation.Status = "Rejected";
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Zaproszenie zosta³o odrzucone.";
            return RedirectToAction("Invitations");
        }



        [HttpGet]
        public IActionResult TeamInvitations()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz byæ zalogowany, aby zarz¹dzaæ zaproszeniami.";
                return RedirectToAction("Login", "Account");
            }

            var invitations = _context.TeamInvitations
                .Include(ti => ti.Team)
                    .ThenInclude(t => t.Event)
                .Include(ti => ti.InvitedBy)
                .Where(ti => ti.InvitedUserId == int.Parse(userId))
                .Select(ti => new TeamInvitationViewModel
                {
                    InvitationId = ti.Id,
                    TeamName = ti.Team.TeamName,
                    EventName = ti.Team.Event.Name,
                    InvitedUserEmail = ti.InvitedUser.Email,
                    InvitedByEmail = ti.InvitedBy.Email
                })
                .ToList();

            return View(invitations);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RespondToInvitation(int invitationId, bool accept)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            int parsedUserId = int.Parse(userId);

            var invitation = _context.TeamInvitations
                .Include(i => i.Team)
                .ThenInclude(t => t.TeamMembers)
                .FirstOrDefault(i => i.Id == invitationId);

            if (invitation == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono zaproszenia.";
                return RedirectToAction("TeamInvitations");
            }

            if (accept)
            {
                if (invitation.Team.TeamMembers.Count >= invitation.Team.Capacity)
                {
                    TempData["ErrorMessage"] = "Dru¿yna osi¹gnê³a maksymaln¹ liczbê cz³onków. Nie mo¿esz do niej do³¹czyæ.";
                    return RedirectToAction("TeamInvitations");
                }

                var newMember = new TeamMember
                {
                    TeamId = invitation.TeamId,
                    UserId = parsedUserId
                };

                _context.TeamMembers.Add(newMember);
                TempData["SuccessMessage"] = "Do³¹czy³eœ do dru¿yny!";
            }
            else
            {
                TempData["ErrorMessage"] = "Odrzuci³eœ zaproszenie.";
            }

            _context.TeamInvitations.Remove(invitation);
            _context.SaveChanges();

            return RedirectToAction("TeamInvitations");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTeam(int teamId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Musisz byæ zalogowany, aby usun¹æ dru¿ynê.";
                return RedirectToAction("TeamParticipants", new { teamId });
            }

            var team = _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.Event)
                .FirstOrDefault(t => t.Id == teamId);

            if (team == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono dru¿yny.";
                return RedirectToAction("TeamParticipants", new { teamId });
            }

            if (team.CaptainId != int.Parse(userId))
            {
                TempData["ErrorMessage"] = "Tylko kapitan mo¿e usun¹æ dru¿ynê.";
                return RedirectToAction("TeamParticipants", new { teamId });
            }

            if (team.Event != null)
            {
                team.Event.OccupiedSeats--;
            }

            _context.TeamMembers.RemoveRange(team.TeamMembers);

            _context.Teams.Remove(team);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Dru¿yna zosta³a pomyœlnie usuniêta.";
            return RedirectToAction("MyEvents");
        }
    }
}