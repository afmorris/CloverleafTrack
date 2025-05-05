namespace CloverleafTrack.Services.Interfaces;

public interface ISeasonService
{
    public Task<int> GetCurrentSeasonAsync();
}