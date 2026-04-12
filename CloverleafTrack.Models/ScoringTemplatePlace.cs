namespace CloverleafTrack.Models;

public class ScoringTemplatePlace
{
    public int Id { get; set; }
    public int ScoringTemplateId { get; set; }
    public int Place { get; set; }
    public decimal Points { get; set; }
}
