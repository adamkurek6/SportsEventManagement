namespace SportsEventManagement.ViewModels
{
    public class MyEventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string ParticipationType { get; set; } 
        public int? TeamId { get; set; } 
        public bool IsCaptain { get; set; } 
        public int CurrentMembers { get; set; }
        public int TeamCapacity { get; set; } 
        public List<TeamMemberViewModel> TeamMembers { get; set; } = new List<TeamMemberViewModel>();


    }
    public class TeamMemberViewModel
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
