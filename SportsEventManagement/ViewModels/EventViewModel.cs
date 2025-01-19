using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportsEventManagement.ViewModels
{
    public class EventViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Nazwa wydarzenia jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa wydarzenia może mieć maksymalnie 100 znaków.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Dyscyplina jest wymagana.")]
        [StringLength(100, ErrorMessage = "Dyscyplina może mieć maksymalnie 100 znaków.")]
        public string Discipline { get; set; }

        [Required(ErrorMessage = "Miejsce wydarzenia jest wymagane.")]
        [StringLength(200, ErrorMessage = "Miejsce wydarzenia może mieć maksymalnie 200 znaków.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Data wydarzenia jest wymagana.")]
        [CustomValidation(typeof(EventViewModel), nameof(ValidateFutureDate))]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Opis wydarzenia jest wymagany.")]
        [StringLength(6000, ErrorMessage = "Opis może mieć maksymalnie 6000 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Podaj maksymalną liczbę miejsc.")]
        [Range(1, 1000, ErrorMessage = "Liczba miejsc musi mieścić się w przedziale od 1 do 1000.")]
        public int Capacity { get; set; }

        [Range(1, 30, ErrorMessage = "Liczba osób w drużynie musi być większa od 1 i mniejsza od 30.")]
        public int? TeamSize { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia rejestracji jest wymagana.")]
        public DateTime RegistrationStart { get; set; }

        [Required(ErrorMessage = "Data zakończenia rejestracji jest wymagana.")]
        public DateTime RegistrationEnd { get; set; }

        [Required(ErrorMessage = "Wybierz typ uczestnictwa.")]
        [RegularExpression("^(Individual|Team)$", ErrorMessage = "Nieprawidłowy typ uczestnika. Dozwolone wartości to: 'Individual' lub 'Team'.")]
        public string ParticipationType { get; set; }
        public bool IsArchived { get; set; }

        public int Id { get; set; }
        public string RegistrationStatus
        {
            get
            {
                if (DateTime.Now < RegistrationStart)
                {
                    var timeUntilStart = RegistrationStart - DateTime.Now;
                    return $"Start rejestracji za {GetTimeDescription(timeUntilStart)}";
                }
                else if (DateTime.Now < RegistrationEnd)
                {
                    var timeUntilEnd = RegistrationEnd - DateTime.Now;
                    return $"Koniec rejestracji za {GetTimeDescription(timeUntilEnd)}";
                }
                else
                {
                    return "Rejestracja zamknięta";
                }
            }
        }
        private static string GetTimeDescription(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                int days = (int)timeSpan.TotalDays;
                int hours = timeSpan.Hours;
                return $"{days} {DeclineWord(days, "dzień", "dni", "dni")} i {hours} {DeclineWord(hours, "godzina", "godziny", "godzin")}";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                int hours = (int)timeSpan.TotalHours;
                int minutes = timeSpan.Minutes;
                return $"{hours} {DeclineWord(hours, "godzina", "godziny", "godzin")} i {minutes} {DeclineWord(minutes, "minuta", "minuty", "minut")}";
            }
            else
            {
                int minutes = timeSpan.Minutes;
                return $"{minutes} {DeclineWord(minutes, "minuta", "minuty", "minut")}";
            }
        }

        private static string DeclineWord(int value, string singular, string plural, string pluralGenitive)
        {
            if (value == 1) return singular;
            if (value >= 2 && value <= 4) return plural;
            return pluralGenitive;
        }



        public static ValidationResult ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date <= DateTime.Now)
            {
                return new ValidationResult("Data wydarzenia musi być w przyszłości.");
            }
            return ValidationResult.Success;
        }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RegistrationStart < DateTime.Now)
            {
                yield return new ValidationResult(
                    "Data rozpoczęcia rejestracji musi być w przyszłości.",
                    new[] { nameof(RegistrationStart) });
            }

            if (RegistrationStart >= RegistrationEnd)
            {
                yield return new ValidationResult(
                    "Data rozpoczęcia rejestracji musi być wcześniejsza niż data zakończenia rejestracji.",
                    new[] { nameof(RegistrationStart), nameof(RegistrationEnd) });
            }

            if (RegistrationEnd >= Date)
            {
                yield return new ValidationResult(
                    "Data zakończenia rejestracji musi być wcześniejsza niż data wydarzenia.",
                    new[] { nameof(RegistrationEnd), nameof(Date) });
            }

            if (RegistrationStart >= Date)
            {
                yield return new ValidationResult(
                    "Data rozpoczęcia rejestracji musi być wcześniejsza niż data wydarzenia.",
                    new[] { nameof(RegistrationStart), nameof(Date) });
            }

            if (ParticipationType == "Team" && (!TeamSize.HasValue || TeamSize < 2 || TeamSize > 30))
            {
                yield return new ValidationResult(
                    "Dla drużyn należy podać liczbę osób w drużynie w przedziale od 2 do 30.",
                    new[] { nameof(TeamSize) });
            }
        }
    }
}
