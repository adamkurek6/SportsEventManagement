namespace SportsEventManagement.ViewModels
{
    public class ParticipantViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }      
        public string FullName => $"{FirstName} {LastName}";
    }
}
