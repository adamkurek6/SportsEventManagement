using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsEventManagement.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa wydarzenia jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa wydarzenia może mieć maksymalnie 100 znaków.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Data i godzina wydarzenia są wymagane.")]
        [DataType(DataType.DateTime, ErrorMessage = "Nieprawidłowy format daty.")]
        [CustomValidation(typeof(Event), nameof(ValidateEventDate))]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Lokalizacja jest wymagana.")]
        [StringLength(200, ErrorMessage = "Lokalizacja może mieć maksymalnie 200 znaków.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Opis wydarzenia jest wymagany.")]
        [StringLength(10000, ErrorMessage = "Opis wydarzenia może mieć maksymalnie 10000 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Liczba miejsc jest wymagana.")]
        [Range(1, 1000, ErrorMessage = "Liczba miejsc musi mieścić się w przedziale od 1 do 1000.")]
        public int Capacity { get; set; }

        public int OccupiedSeats { get; set; } = 0;

        [Required(ErrorMessage = "Data rozpoczęcia rejestracji jest wymagana.")]
        [DataType(DataType.DateTime, ErrorMessage = "Nieprawidłowy format daty.")]
        [CustomValidation(typeof(Event), nameof(ValidateRegistrationStart))]
        public DateTime RegistrationStart { get; set; }

        [Required(ErrorMessage = "Data zakończenia rejestracji jest wymagana.")]
        [DataType(DataType.DateTime, ErrorMessage = "Nieprawidłowy format daty.")]
        [CustomValidation(typeof(Event), nameof(ValidateRegistrationEnd))]
        public DateTime RegistrationEnd { get; set; }

        [Required(ErrorMessage = "Wybierz typ uczestników.")]
        [RegularExpression("^(Individual|Team)$", ErrorMessage = "Nieprawidłowy typ uczestnika. Dozwolone wartości to: 'Individual' lub 'Team'.")]
        public string ParticipationType { get; set; }

        [Range(1, 30, ErrorMessage = "Liczba uczestników w drużynie musi mieścić się w przedziale od 1 do 30.")]
        public int? TeamSize { get; set; }

        [Required(ErrorMessage = "Dyscyplina jest wymagana.")]
        [StringLength(100, ErrorMessage = "Dyscyplina może mieć maksymalnie 100 znaków.")]
        public string Discipline { get; set; }

        public bool IsArchived { get; set; } = false;
        public bool IsInactive { get; set; } = false;

        public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();

        public int HostId { get; set; }

        [ForeignKey("HostId")]
        public User Host { get; set; }

        [NotMapped]
        public string RegistrationStatus
        {
            get
            {
                if (RegistrationStart > DateTime.Now)
                {
                    var diff = RegistrationStart - DateTime.Now;
                    return $"Start rejestracji za {diff.Days} dni i {diff.Hours} godzin";
                }
                else if (RegistrationEnd > DateTime.Now)
                {
                    var diff = RegistrationEnd - DateTime.Now;
                    return $"Koniec rejestracji za {diff.Days} dni i {diff.Hours} godzin";
                }
                return "Rejestracja zakończona";
            }
        }

        public int RegisteredParticipants { get; set; } = 0;
        public int MaxParticipants => Capacity;
        public string CompetitionType => ParticipationType == "Individual" ? "Indywidualne" : "Drużynowe";

        public static ValidationResult ValidateEventDate(DateTime date, ValidationContext context)
        {
            if (date <= DateTime.Now)
            {
                return new ValidationResult("Data wydarzenia musi być w przyszłości.");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult ValidateRegistrationStart(DateTime start, ValidationContext context)
        {
            var instance = (Event)context.ObjectInstance;
            if (start >= instance.RegistrationEnd || start >= instance.Date)
            {
                return new ValidationResult("Data rozpoczęcia rejestracji musi być wcześniejsza od zakończenia i daty wydarzenia.");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult ValidateRegistrationEnd(DateTime end, ValidationContext context)
        {
            var instance = (Event)context.ObjectInstance;
            if (end <= instance.RegistrationStart || end >= instance.Date)
            {
                return new ValidationResult("Data zakończenia rejestracji musi być pomiędzy datą rozpoczęcia a datą wydarzenia.");
            }
            return ValidationResult.Success;
        }
    }
}
