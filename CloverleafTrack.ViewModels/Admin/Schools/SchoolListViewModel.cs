namespace CloverleafTrack.ViewModels.Admin.Schools;

public class SchoolsIndexViewModel
{
    public List<SchoolRowViewModel> Schools { get; set; } = new();
}

public class SchoolRowViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
}
