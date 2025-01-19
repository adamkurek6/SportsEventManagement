using System;
using System.ComponentModel.DataAnnotations;

namespace SportsEventManagement.ViewModels
{
    public class EventEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa wydarzenia jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa wydarzenia może mieć maksymalnie 100 znaków.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Dyscyplina jest wymagana.")]
        [StringLength(100, ErrorMessage = "Dyscyplina może mieć maksymalnie 100 znaków.")]
        public string Discipline { get; set; }

        [Required(ErrorMessage = "Lokalizacja jest wymagana.")]
        [StringLength(200, ErrorMessage = "Lokalizacja może mieć maksymalnie 200 znaków.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Opis wydarzenia jest wymagany.")]
        [StringLength(10000, ErrorMessage = "Opis wydarzenia może mieć maksymalnie 10000 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia rejestracji jest wymagana.")]
        [CustomValidation(typeof(EventEditViewModel), nameof(ValidateRegistrationStart))]
        public DateTime RegistrationStart { get; set; }

        [Required(ErrorMessage = "Data zakończenia rejestracji jest wymagana.")]
        [CustomValidation(typeof(EventEditViewModel), nameof(ValidateRegistrationEnd))]
        public DateTime RegistrationEnd { get; set; }

        [Required(ErrorMessage = "Data wydarzenia jest wymagana.")]
        [CustomValidation(typeof(EventEditViewModel), nameof(ValidateEventDate))]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Liczba miejsc jest wymagana.")]
        [Range(1, 1000, ErrorMessage = "Liczba miejsc musi mieścić się w przedziale od 1 do 1000.")]
        public int Capacity { get; set; }

        [Range(1, 30, ErrorMessage = "Liczba uczestników w drużynie musi mieścić się w przedziale od 1 do 30.")]
        public int? TeamSize { get; set; }

        [Required(ErrorMessage = "Wybierz typ uczestników.")]
        public string ParticipationType { get; set; }

        public bool ShouldValidateParticipationType => IsRegistrationStartEditable;


        public bool IsRegistrationStartEditable => RegistrationStart >= DateTime.Now;
        public bool IsRegistrationEndEditable => RegistrationEnd >= DateTime.Now;
        public bool IsEventDateEditable => Date >= DateTime.Now;
        public bool IsRegistrationClosed => RegistrationEnd < DateTime.Now;

        public static ValidationResult ValidateRegistrationStart(object value, ValidationContext context)
        {
            var model = (EventEditViewModel)context.ObjectInstance;
            var registrationStart = (DateTime)value;

            if (registrationStart > model.RegistrationEnd)
            {
                return new ValidationResult("Data rozpoczęcia rejestracji nie może być późniejsza niż data zakończenia rejestracji.");
            }

            if (registrationStart < DateTime.Now && model.IsRegistrationStartEditable)
            {
                return new ValidationResult("Data rozpoczęcia rejestracji nie może być w przeszłości.");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateRegistrationEnd(object value, ValidationContext context)
        {
            var model = (EventEditViewModel)context.ObjectInstance;
            var registrationEnd = (DateTime)value;

            if (registrationEnd <= model.RegistrationStart)
            {
                return new ValidationResult("Data zakończenia rejestracji musi być późniejsza niż data rozpoczęcia rejestracji.");
            }

            if (registrationEnd >= model.Date)
            {
                return new ValidationResult("Data zakończenia rejestracji musi być przed datą wydarzenia.");
            }

            if (registrationEnd < DateTime.Now && model.IsRegistrationEndEditable)
            {
                return new ValidationResult("Data zakończenia rejestracji nie może być w przeszłości.");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateEventDate(object value, ValidationContext context)
        {
            var model = (EventEditViewModel)context.ObjectInstance;
            var eventDate = (DateTime)value;

            if (eventDate <= model.RegistrationEnd)
            {
                return new ValidationResult("Data wydarzenia musi być późniejsza niż data zakończenia rejestracji.");
            }

            if (eventDate < DateTime.Now && model.IsEventDateEditable)
            {
                return new ValidationResult("Data wydarzenia nie może być w przeszłości.");
            }

            return ValidationResult.Success;
        }
    }
}
