using System.Collections.ObjectModel;
using CMMS_QRCode_Scanner.Models;

namespace CMMS_QRCode_Scanner.Services
{
    public interface IRecentScanService
    {
        ObservableCollection<RecentScanItem> RecentScans { get; }
        void AddScan(string barcodeId, string description);
        void Load();
    }
}
