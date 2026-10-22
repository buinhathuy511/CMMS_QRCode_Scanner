using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Storage;
using CMMS_QRCode_Scanner.Models;

namespace CMMS_QRCode_Scanner.Services
{
    public class RecentScanService : IRecentScanService
    {
        private readonly IAuthService _authService;
        public ObservableCollection<RecentScanItem> RecentScans { get; } = new();

        private string PrefKey => $"RecentScansHistory_{_authService.CurrentUsername ?? "guest"}";

        public RecentScanService(IAuthService authService)
        {
            _authService = authService;
            // Dữ liệu sẽ được load khi gọi Load() từ LoginViewModel
        }

        public void AddScan(string barcodeId, string description)
        {
            var existing = RecentScans.FirstOrDefault(x => x.BarcodeId == barcodeId);
            if (existing != null)
            {
                RecentScans.Remove(existing);
            }

            RecentScans.Insert(0, new RecentScanItem
            {
                BarcodeId = barcodeId,
                Description = description,
                ScannedAt = DateTime.Now
            });

            if (RecentScans.Count > 5)
            {
                RecentScans.RemoveAt(RecentScans.Count - 1);
            }

            SaveToPreferences();
        }

        public void Load()
        {
            RecentScans.Clear();
            var json = Preferences.Default.Get(PrefKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<RecentScanItem[]>(json);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            RecentScans.Add(item);
                        }
                    }
                }
                catch { }
            }
        }

        private void SaveToPreferences()
        {
            var json = JsonSerializer.Serialize(RecentScans.ToArray());
            Preferences.Default.Set(PrefKey, json);
        }
    }
}
