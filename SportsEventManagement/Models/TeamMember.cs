using SportsEventManagement.Models;

public class TeamMember
{
    public int Id { get; set; } // Identyfikator
    public int TeamId { get; set; } // Id drużyny
    public Team Team { get; set; } // Relacja do drużyny
    public bool IsCaptain { get; set; } = false;
    public int UserId { get; set; } // Id użytkownika
    public User User { get; set; } // Relacja do użytkownika
}
