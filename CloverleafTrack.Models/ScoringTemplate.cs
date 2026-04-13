namespace CloverleafTrack.Models;

public class ScoringTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }

    public List<ScoringTemplatePlace> Places { get; set; } = new();
}
