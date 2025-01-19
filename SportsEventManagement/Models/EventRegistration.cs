namespace SportsEventManagement.Models
{
    public class EventRegistration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int? UserId { get; set; } 
        public User User { get; set; }
        public int? TeamId { get; set; } 
        public string? TeamName { get; set; } 
        public DateTime RegistrationDate { get; set; } = DateTime.Now; 
    }
}
