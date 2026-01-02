using CloverleafTrack.ViewModels.Home;

namespace CloverleafTrack.Services.Interfaces;

public interface IHomeService
{
    Task<HomePageViewModel> GetHomePageDataAsync();
}
