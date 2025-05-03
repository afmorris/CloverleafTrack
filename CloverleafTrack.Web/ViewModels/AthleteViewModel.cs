namespace CloverleafTrack.Web.ViewModels;

public class AthleteViewModel
{
    public int Id { get; set; }
    private string FullName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public string Gender { get; set; } = string.Empty;
}