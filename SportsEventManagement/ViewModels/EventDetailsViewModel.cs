namespace SportsEventManagement.ViewModels
{
    public class EventDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Description { get; set; }
        public string Discipline { get; set; }
        public bool RequiresTeam { get; set; }
        public int? TeamSize { get; set; }
        public int RegisteredParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public string ParticipationType { get; set; }
        public int OccupiedSeats { get; set; }
        public int Capacity { get; set; }
        public int? CaptainId { get; set; }
        public string CompetitionType { get; set; }
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
    }
}

