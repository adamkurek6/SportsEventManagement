using SportsEventManagement.Models;

public class Team
{
    public int Id { get; set; }
    public string TeamName { get; set; }
    public int Capacity { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; }
    public int CaptainId { get; set; }
    public User Captain { get; set; }
    public ICollection<TeamMember> TeamMembers { get; set; }
    public ICollection<User> Members { get; set; } = new List<User>();
    public ICollection<TeamInvitation> TeamInvitations { get; set; }


}
