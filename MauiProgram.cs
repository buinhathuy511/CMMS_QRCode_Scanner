using Microsoft.Extensions.Logging;

using ZXing.Net.Maui.Controls;

namespace CMMS_QRCode_Scanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<CMMS_QRCode_Scanner.Services.IAuthService, CMMS_QRCode_Scanner.Services.ApiAuthService>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Authentication.LoginViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Authentication.LoginPage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Dashboard.HomeViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Dashboard.HomePage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.ScannerMenu.ScannerMenuViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.ScannerMenu.ScannerMenuPage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.CameraScan.QrScannerViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.CameraScan.QrScannerPage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.ManualEntry.ManualIdViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.ManualEntry.ManualIdPage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.ImageScan.ScanFromImageViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Scanner.ImageScan.ScanFromImagePage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Inventory.DummyResultViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Inventory.DummyResultPage>();

        builder.Services.AddSingleton<CMMS_QRCode_Scanner.Services.IMockDatabaseService, CMMS_QRCode_Scanner.Services.ApiDatabaseService>();
        builder.Services.AddSingleton<CMMS_QRCode_Scanner.Services.IRecentScanService, CMMS_QRCode_Scanner.Services.RecentScanService>();
        
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Inventory.ItemInfoViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Inventory.ItemInfoPage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Inventory.EditSparePartViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.Inventory.EditSparePartPage>();

        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.ExportOutbound.ExportSetupViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.ExportOutbound.ExportSetupPage>();
        
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.ExportOutbound.ExportConfirmViewModel>();
        builder.Services.AddTransient<CMMS_QRCode_Scanner.Features.ExportOutbound.ExportConfirmPage>();

		return builder.Build();
	}
}
