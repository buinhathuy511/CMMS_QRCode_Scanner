using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Scanner.ImageScan
{
    public partial class ScanFromImageViewModel : BaseViewModel
    {
        public ScanFromImageViewModel()
        {
            Title = "Scan from Image";
        }

        [ObservableProperty]
        private bool hasImage;

        [RelayCommand]
        public async Task SelectImageAsync()
        {
            IsBusy = true;
            try
            {
                var result = await Microsoft.Maui.Media.MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    HasImage = true;
                    
                    using var stream = await result.OpenReadAsync();
                    using var bitmap = SkiaSharp.SKBitmap.Decode(stream);
                    
                    if (bitmap != null)
                    {
                        var reader = new ZXing.SkiaSharp.BarcodeReader();
                        reader.Options = new ZXing.Common.DecodingOptions
                        {
                            TryHarder = true,
                            PossibleFormats = new[] { ZXing.BarcodeFormat.QR_CODE, ZXing.BarcodeFormat.CODE_128, ZXing.BarcodeFormat.CODE_39 }
                        };

                        var barcodeResult = reader.Decode(bitmap);
                        if (barcodeResult != null && !string.IsNullOrEmpty(barcodeResult.Text))
                        {
                            await Shell.Current.GoToAsync($"ItemInfo?BarcodeId={barcodeResult.Text}");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy mã vạch hoặc QR Code trong ảnh.", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể đọc ảnh.", "OK");
                    }
                    
                    HasImage = false; // Reset
                }
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            IsBusy = false;
        }
    }
}

