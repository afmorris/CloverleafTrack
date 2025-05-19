using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

public class Athlete
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int GraduationYear { get; set; }
    public bool IsActive { get; set; }

    public List<Event> EventParticipations { get; set; } = new();
}