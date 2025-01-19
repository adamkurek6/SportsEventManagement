public class PastTeam
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public string CaptainName { get; set; } 
    public DateTime DissolvedDate { get; set; } 
    public List<PastTeamMember> Members { get; set; } = new List<PastTeamMember>(); 
}

public class PastTeamMember
{
    public int Id { get; set; }
    public int PastTeamId { get; set; } 
    public string FullName { get; set; } 
    public string Email { get; set; } 
}
