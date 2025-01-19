using System.ComponentModel.DataAnnotations;

namespace SportsEventManagement.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię")]
        [RegularExpression(@"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*$", ErrorMessage = "Imię musi zaczynać się wielką literą i zawierać tylko litery.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        [RegularExpression(@"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*$", ErrorMessage = "Nazwisko musi zaczynać się wielką literą i zawierać tylko litery.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Adres e-mail jest wymagany")]
        [EmailAddress(ErrorMessage = "Wprowadź prawidłowy adres e-mail")]
        [Display(Name = "Adres e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Hasło musi mieć co najmniej 8 znaków.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Hasło musi zawierać co najmniej jedną wielką literę, jedną małą literę, jedną cyfrę oraz jeden znak specjalny.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potwierdź hasło")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są zgodne.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Display(Name = "Numer telefonu")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Numer telefonu musi składać się z 9 cyfr.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Wybierz typ konta")]
        [Display(Name = "Typ konta")]
        [RegularExpression("^(User|Host)$", ErrorMessage = "Nieprawidłowy typ konta.")]
        public string AccountType { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(FirstName))
            {
                FirstName = char.ToUpper(FirstName[0]) + FirstName.Substring(1).ToLower();
            }

            if (!string.IsNullOrEmpty(LastName))
            {
                LastName = char.ToUpper(LastName[0]) + LastName.Substring(1).ToLower();
            }

            return new List<ValidationResult>();
        }
    }
}
