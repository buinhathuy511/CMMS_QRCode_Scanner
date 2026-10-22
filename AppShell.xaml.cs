namespace CMMS_QRCode_Scanner;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("ScannerMenu", typeof(Features.Scanner.ScannerMenu.ScannerMenuPage));
        Routing.RegisterRoute("QrScanner", typeof(Features.Scanner.CameraScan.QrScannerPage));
        Routing.RegisterRoute("ManualId", typeof(Features.Scanner.ManualEntry.ManualIdPage));
        Routing.RegisterRoute("ScanFromImage", typeof(Features.Scanner.ImageScan.ScanFromImagePage));
        Routing.RegisterRoute("DummyResult", typeof(Features.Inventory.DummyResultPage));
        Routing.RegisterRoute("ItemInfo", typeof(Features.Inventory.ItemInfoPage));
        Routing.RegisterRoute("EditSparePart", typeof(Features.Inventory.EditSparePartPage));
        
        Routing.RegisterRoute("ExportSetup", typeof(Features.ExportOutbound.ExportSetupPage));
        Routing.RegisterRoute("ExportConfirm", typeof(Features.ExportOutbound.ExportConfirmPage));
	}
}
