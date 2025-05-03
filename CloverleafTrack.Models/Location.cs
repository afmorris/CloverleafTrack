namespace CloverleafTrack.Models;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "USA";
}