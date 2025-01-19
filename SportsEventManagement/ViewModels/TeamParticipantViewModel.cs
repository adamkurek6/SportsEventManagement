namespace SportsEventManagement.ViewModels
{
    public class TeamParticipantViewModel
    {
        public string TeamName { get; set; }
        public List<MemberViewModel> Members { get; set; }
    }

    public class MemberViewModel
    {
        public string Name { get; set; }
        public bool IsCaptain { get; set; }
    }

}
