using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CMMS_QRCode_Scanner.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Dashboard
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        public HomeViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Home";
        }

        public string WelcomeMessage => $"Welcome, {_authService.CurrentUsername}";

        [RelayCommand]
        public async Task GoToViewEditAsync()
        {
            await Shell.Current.GoToAsync("ScannerMenu");
        }

        [RelayCommand]
        public async Task GoToExportOutboundAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("ExportSetup");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi Mở Export Outbound", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            _authService.Logout();
            await Shell.Current.GoToAsync("//Login");
        }
    }
}

