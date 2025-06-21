using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IMeetRepository
{
    public Task<List<Meet>> GetMeetsForSeasonAsync(int seasonId);
}