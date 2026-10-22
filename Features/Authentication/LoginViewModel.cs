using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMS_QRCode_Scanner.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Authentication
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IRecentScanService _recentScanService;

        public LoginViewModel(IAuthService authService, IRecentScanService recentScanService)
        {
            _authService = authService;
            _recentScanService = recentScanService;
            Title = "Login";
        }

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isPasswordHidden = true;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool hasError;

        [RelayCommand]
        public void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username is required.";
                HasError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                HasError = true;
                return;
            }

            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var result = await _authService.LoginAsync(Username, Password);
                
                if (result.Success)
                {
                    _recentScanService.Load();
                    // Navigate to Home
                    await Shell.Current.GoToAsync("//Home");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Invalid username or password.";
                    HasError = true;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

