using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Scanner.ImageScan
{
    [QueryProperty(nameof(Mode), "Mode")]
    public partial class ScanFromImageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string mode = string.Empty;

        public ScanFromImageViewModel()
        {
            Title = "Scan from Image";
        }

        partial void OnModeChanged(string value)
        {
            if (string.Equals(value, "export", System.StringComparison.OrdinalIgnoreCase))
            {
                Title = "Quét Ảnh QR (Xuất Kho)";
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoImage))]
        private bool hasImage;

        public bool HasNoImage => !HasImage;

        [RelayCommand]
        public async Task SelectImageAsync()
        {
            if (IsBusy) return;
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
                            var trimmedCode = barcodeResult.Text.Trim();
                            if (string.Equals(Mode, "export", System.StringComparison.OrdinalIgnoreCase))
                            {
                                // Return to ExportSetupPage with the scanned barcode from image
                                await Shell.Current.GoToAsync($"..?ScannedBarcodeId={Uri.EscapeDataString(trimmedCode)}");
                            }
                            else
                            {
                                await Shell.Current.GoToAsync($"ItemInfo?BarcodeId={Uri.EscapeDataString(trimmedCode)}");
                            }
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy mã vạch hoặc QR Code trong ảnh.", "OK");
                        }
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Lỗi", "Không thể đọc ảnh.", "OK");
                    }
                    
                    HasImage = false; // Reset
                }
            }
            catch (System.Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

