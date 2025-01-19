namespace SportsEventManagement.ViewModels
{
    public class ManageTeamViewModel
    {
        public int TeamId { get; set; }
        public int CaptainId { get; set; } 
        public List<TeamMemberViewModel> TeamMembers { get; set; }
    }
}
