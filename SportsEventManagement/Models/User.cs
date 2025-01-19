using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsEventManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50, ErrorMessage = "Imię nie może być dłuższe niż 50 znaków.")]
        [RegularExpression(@"^[A-Za-zżźćńółęąśŻŹĆĄŚĘŁÓŃ]+$", ErrorMessage = "Imię może zawierać tylko litery.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(50, ErrorMessage = "Nazwisko nie może być dłuższe niż 50 znaków.")]
        [RegularExpression(@"^[A-Za-zżźćńółęąśŻŹĆĄŚĘŁÓŃ]+$", ErrorMessage = "Nazwisko może zawierać tylko litery.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[A-Za-zżźćńółęąśŻŹĆĄŚĘŁÓŃ])(?=.*[A-ZŻŹĆĄŚĘŁÓŃ])(?=.*\d)(?=.*\W).+$", ErrorMessage = "Hasło musi zawierać co najmniej jedną wielką literę, jedną małą literę, cyfrę i znak specjalny.")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Podaj prawidłowy numer telefonu.")]
        [Display(Name = "Numer telefonu")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Wybierz typ konta.")]
        [Display(Name = "Typ konta")]
        [RegularExpression("^(User|Host)$", ErrorMessage = "Nieprawidłowy typ konta. Dostępne wartości to: 'User' lub 'Host'.")]
        public string AccountType { get; set; }
        public int? TeamId { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; }
        public string? Name { get; set; }
        public ICollection<TeamInvitation> Invitations { get; set; } = new List<TeamInvitation>();
    }
}
