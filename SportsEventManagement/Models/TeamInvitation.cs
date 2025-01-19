namespace SportsEventManagement.Models
{
    public class TeamInvitation
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public int InvitedUserId { get; set; }
        public User InvitedUser { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Accepted", "Rejected"
        public DateTime InvitationDate { get; set; } // Data zaproszenia
        public int InvitedById { get; set; } // Kto wysłał zaproszenie
        public User InvitedBy { get; set; } // Nawigacja do zapraszającego
        public DateTime SentDate { get; set; } // Data wysłania zaproszenia
        public DateTime CreatedAt { get; set; }

    }
}
