namespace SportsEventManagement.ViewModels
{
    public class TeamInvitationViewModel
    {
        public int InvitationId { get; set; }
        public string TeamName { get; set; }
        public string EventName { get; set; }
        public string CaptainName { get; set; }
        public string InvitedUserEmail { get; set; } 
        public string InvitedByEmail { get; set; } 
    }
}
