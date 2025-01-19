using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsEventManagement.Data;
using SportsEventManagement.Models;
using Microsoft.AspNetCore.Http;
using SportsEventManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }



    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Adres e-mail jest już zajęty.");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                AccountType = model.AccountType,
                Name = $"{model.FirstName} {model.LastName}",
                PhoneNumber = model.PhoneNumber ?? "Brak numeru"
            };



            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login", "Account");
        }

        return View(model);
    }



    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }



    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(string.Empty, "Podaj adres e-mail i hasło.");
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.FirstName);
                HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");
                HttpContext.Session.SetString("AccountType", user.AccountType);

                _logger.LogInformation("Użytkownik {Email} zalogował się pomyślnie. Typ konta: {AccountType}", model.Email, user.AccountType);

                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("Nieudana próba logowania dla e-maila: {Email}", model.Email);

            ModelState.AddModelError(string.Empty, "Nieprawidłowy adres e-mail lub hasło.");
        }

        return View(model);
    }



    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }



    public IActionResult Profile()
    {
        return View();
    }



    [HttpGet]
    public JsonResult IsLoggedIn()
    {
        var isLoggedIn = HttpContext.Session.GetString("UserId") != null;
        return Json(new { isLoggedIn });
    }



    [HttpGet]
    public IActionResult MyEvents()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby zobaczyć swoje wydarzenia.";
            return RedirectToAction("MainPage", "Home");
        }

        int parsedUserId = int.Parse(userId);

        var individualEvents = _context.EventRegistrations
            .Include(er => er.Event)
            .Where(er => er.UserId == parsedUserId)
            .Select(er => new MyEventViewModel
            {
                EventId = er.Event.Id,
                EventName = er.Event.Name,
                EventDate = er.Event.Date,
                ParticipationType = "Indywidualne",
                TeamId = null
            })
            .ToList();


        var teamEvents = _context.TeamMembers
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Event)
            .Include(tm => tm.Team.TeamMembers)
            .Include(tm => tm.User)
            .Where(tm => tm.UserId == parsedUserId)
            .Select(tm => new MyEventViewModel
            {
                TeamId = tm.TeamId,
                EventId = tm.Team.Event.Id,
                EventName = tm.Team.Event.Name,
                EventDate = tm.Team.Event.Date,
                ParticipationType = "Drużynowe",
                IsCaptain = tm.Team.CaptainId == parsedUserId,
                TeamMembers = tm.Team.TeamMembers.Select(member => new TeamMemberViewModel
                {
                    Id = member.UserId,
                    Name = member.User.FirstName + " " + member.User.LastName,
                    Email = member.User.Email
                }).ToList(),
                CurrentMembers = tm.Team.TeamMembers.Count,
                TeamCapacity = tm.Team.Capacity
            })
            .ToList();

        var allEvents = individualEvents.Union(teamEvents).ToList();

        var today = DateTime.Now;
        ViewBag.ActiveEvents = allEvents.Where(e => e.EventDate >= today).ToList();
        ViewBag.ArchivedEvents = allEvents.Where(e => e.EventDate < today).ToList();

        return View();
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LeaveEvent(int eventId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby opuścić wydarzenie.";
            return RedirectToAction("Login", "Account");
        }

        var eventRegistration = _context.EventRegistrations
            .FirstOrDefault(er => er.EventId == eventId && er.UserId == int.Parse(userId));

        if (eventRegistration == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono twojej rejestracji na to wydarzenie.";
            return RedirectToAction("MyEvents");
        }

        var eventDetails = _context.Events.FirstOrDefault(e => e.Id == eventId);
        if (eventDetails == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono wydarzenia.";
            return RedirectToAction("MyEvents");
        }

        if (DateTime.Now > eventDetails.RegistrationEnd)
        {
            TempData["ErrorMessage"] = "Nie możesz opuścić wydarzenia po zakończeniu rejestracji.";
            return RedirectToAction("MyEvents");
        }

        _context.EventRegistrations.Remove(eventRegistration);

        if (eventDetails.OccupiedSeats > 0)
        {
            eventDetails.OccupiedSeats--;
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Pomyślnie opuściłeś wydarzenie.";
        return RedirectToAction("MyEvents");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResolveTeam(int teamId)
    {
        if (teamId <= 0)
        {
            TempData["ErrorMessage"] = "Nieprawidłowy identyfikator drużyny.";
            return RedirectToAction("MyEvents");
        }

        var team = _context.Teams
            .Include(t => t.TeamMembers)
            .Include(t => t.Event)
            .FirstOrDefault(t => t.Id == teamId);

        if (team == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono drużyny.";
            return RedirectToAction("MyEvents");
        }

        if (DateTime.Now > team.Event.RegistrationEnd)
        {
            TempData["ErrorMessage"] = "Nie możesz rozwiązać drużyny po zakończeniu rejestracji.";
            return RedirectToAction("MyEvents");
        }

        var eventDetails = team.Event;
        eventDetails.OccupiedSeats--;

        foreach (var member in team.TeamMembers)
        {
            var notification = new Notification
            {
                UserId = member.UserId,
                Message = $"Drużyna {team.TeamName} zapisana na wydarzenie {eventDetails.Name} została rozwiązana.",
                NotificationDate = DateTime.Now
            };
            _context.Notifications.Add(notification);
        }

        _context.TeamMembers.RemoveRange(team.TeamMembers);

        _context.Teams.Remove(team);

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Drużyna została rozwiązana.";
        return RedirectToAction("MyEvents");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InvitePlayer(int teamId, string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Adres e-mail nie może być pusty.";
            return RedirectToAction("MyEvents");
        }

        var team = _context.Teams
            .Include(t => t.TeamMembers)
            .Include(t => t.Event)
            .FirstOrDefault(t => t.Id == teamId);

        if (team == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono drużyny.";
            return RedirectToAction("MyEvents");
        }

        if (team.TeamMembers.Count >= team.Capacity)
        {
            TempData["ErrorMessage"] = "Drużyna osiągnęła maksymalną liczbę członków.";
            return RedirectToAction("MyEvents");
        }

        var invitedUser = _context.Users.FirstOrDefault(u => u.Email == email);
        if (invitedUser == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono użytkownika z podanym adresem e-mail.";
            return RedirectToAction("MyEvents");
        }

        var isUserInAnyTeamForEvent = _context.TeamMembers
            .Include(tm => tm.Team)
            .Any(tm => tm.UserId == invitedUser.Id && tm.Team.EventId == team.EventId);

        if (isUserInAnyTeamForEvent)
        {
            TempData["ErrorMessage"] = "Użytkownik jest już członkiem drużyny w tym wydarzeniu.";
            return RedirectToAction("MyEvents");
        }

        var currentUserId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(currentUserId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby wysyłać zaproszenia.";
            return RedirectToAction("Login", "Account");
        }

        int parsedCurrentUserId = int.Parse(currentUserId);

        _context.TeamInvitations.Add(new TeamInvitation
        {
            TeamId = teamId,
            InvitedUserId = invitedUser.Id,
            InvitedById = parsedCurrentUserId,
            CreatedAt = DateTime.Now
        });

        var invitingUser = _context.Users.FirstOrDefault(u => u.Id == parsedCurrentUserId);
        if (invitingUser != null)
        {
            var notification = new Notification
            {
                UserId = invitedUser.Id,
                Message = $"{invitingUser.FirstName} zaprosił Cię do drużyny {team.TeamName}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
        }

        _context.SaveChanges();
        TempData["SuccessMessage"] = "Zaproszenie zostało wysłane.";
        return RedirectToAction("MyEvents");

    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LeaveTeam(int teamId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby opuścić drużynę.";
            return RedirectToAction("MyEvents");
        }

        var teamMember = _context.TeamMembers.FirstOrDefault(tm => tm.TeamId == teamId && tm.UserId == int.Parse(userId));
        if (teamMember == null)
        {
            TempData["ErrorMessage"] = "Nie jesteś członkiem tej drużyny.";
            return RedirectToAction("MyEvents");
        }

        var team = _context.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono drużyny.";
            return RedirectToAction("MyEvents");
        }

        _context.TeamMembers.Remove(teamMember);

        var leavingUser = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
        if (leavingUser != null)
        {
            var notification = new Notification
            {
                UserId = team.CaptainId,
                Message = $"{leavingUser.FirstName} {leavingUser.LastName} opuścił drużynę {team.TeamName}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Pomyślnie opuściłeś drużynę.";
        return RedirectToAction("MyEvents");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveTeamMember(int teamId, int memberId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby zarządzać drużyną.";
            return RedirectToAction("Login", "Account");
        }

        int parsedUserId = int.Parse(userId);

        var team = _context.Teams
            .Include(t => t.TeamMembers)
            .FirstOrDefault(t => t.Id == teamId);

        if (team == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono drużyny.";
            return RedirectToAction("MyEvents");
        }

        if (team.CaptainId != parsedUserId)
        {
            TempData["ErrorMessage"] = "Tylko kapitan drużyny może usuwać członków.";
            return RedirectToAction("ManageTeam", new { teamId });
        }

        if (team.CaptainId == memberId)
        {
            TempData["ErrorMessage"] = "Nie możesz usunąć siebie jako kapitana.";
            return RedirectToAction("ManageTeam", new { teamId });
        }

        var member = team.TeamMembers.FirstOrDefault(tm => tm.UserId == memberId);
        if (member != null)
        {
            _context.TeamMembers.Remove(member);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Członek drużyny został usunięty.";
        }
        else
        {
            TempData["ErrorMessage"] = "Nie znaleziono członka drużyny.";
        }

        return RedirectToAction("ManageTeam", new { teamId });
    }



    [HttpGet]
    public IActionResult GetTeamMembers(int teamId)
    {
        var teamMembers = _context.TeamMembers
            .Include(tm => tm.User)
            .Where(tm => tm.TeamId == teamId)
            .Select(tm => new
            {
                id = tm.User.Id,
                name = tm.User.FirstName + " " + tm.User.LastName,
                email = tm.User.Email
            })
            .ToList();

        return Json(teamMembers);
    }



    [HttpGet]
    public IActionResult ManageTeam(int teamId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby zarządzać drużyną.";
            return RedirectToAction("MainPage", "Home");
        }

        int parsedUserId = int.Parse(userId);

        var team = _context.Teams
            .Include(t => t.TeamMembers)
            .ThenInclude(tm => tm.User)
            .FirstOrDefault(t => t.Id == teamId);

        if (team == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono drużyny.";
            return RedirectToAction("MyEvents");
        }

        var isUserInTeam = team.TeamMembers.Any(tm => tm.UserId == parsedUserId);
        if (!isUserInTeam)
        {
            TempData["ErrorMessage"] = "Nie masz dostępu do zarządzania tą drużyną.";
            return RedirectToAction("MyEvents");
        }

        var viewModel = new ManageTeamViewModel
        {
            TeamId = team.Id,
            TeamMembers = team.TeamMembers.Select(tm => new TeamMemberViewModel
            {
                Id = tm.UserId,
                Name = tm.User.FirstName + " " + tm.User.LastName,
                Email = tm.User.Email
            }).ToList()
        };

        return View(viewModel);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TransferCaptainRole(int teamId, int newCaptainId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby zarządzać drużyną.";
            return RedirectToAction("Login", "Account");
        }

        int currentCaptainId = int.Parse(userId);

        var team = _context.Teams
            .Include(t => t.TeamMembers)
            .FirstOrDefault(t => t.Id == teamId && t.CaptainId == currentCaptainId);

        if (team == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono drużyny lub nie masz uprawnień do zarządzania tą drużyną.";
            return RedirectToAction("MyEvents");
        }

        if (!team.TeamMembers.Any(tm => tm.UserId == newCaptainId))
        {
            TempData["ErrorMessage"] = "Wybrany użytkownik nie jest członkiem tej drużyny.";
            return RedirectToAction("MyEvents");
        }

        team.CaptainId = newCaptainId;

        var newCaptain = _context.Users.FirstOrDefault(u => u.Id == newCaptainId);
        if (newCaptain != null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = newCaptainId,
                Message = $"Jesteś teraz kapitanem drużyny {team.TeamName}.",
                CreatedAt = DateTime.Now
            });
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Rola kapitana została przekazana.";
        return RedirectToAction("MyEvents");
    }
}
