using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace CMMS_QRCode_Scanner.Shared.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string title;

        public bool IsNotBusy => !IsBusy;

        [RelayCommand]
        public virtual async Task GoBackAsync()
        {
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
        }
    }
}

