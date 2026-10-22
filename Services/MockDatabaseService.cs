using System.Text.Json;
using CMMS_QRCode_Scanner.Models;

namespace CMMS_QRCode_Scanner.Services
{
    public interface IMockDatabaseService
    {
        Task<BaseItem?> GetItemByBarcodeIdAsync(string barcodeId);
        Task<bool> UpdateItemAsync(BaseItem item);
        Task<List<MovementTypeItem>> GetMovementTypesAsync();
        Task<ExportOrderResponse> CreateExportOrderAsync(ExportOrderRequest request);
        Task<List<string>> GetAvailableSerialsAsync(string partCode);
    }

    public class MockDatabaseService : IMockDatabaseService
    {
        private class DatabaseWrapper
        {
            public List<SparePart> SpareParts { get; set; } = new();
            public List<CodedSparePart> CodedSpareParts { get; set; } = new();
            public List<Equipment> Equipment { get; set; } = new();
        }

        private DatabaseWrapper? _dbCache;

        public async Task<BaseItem?> GetItemByBarcodeIdAsync(string barcodeId)
        {
            if (_dbCache == null)
            {
                await LoadDatabaseAsync();
            }

            if (_dbCache == null) return null;

            var sp = _dbCache.SpareParts.FirstOrDefault(x => string.Equals(x.BarcodeId, barcodeId, StringComparison.OrdinalIgnoreCase));
            if (sp != null) return sp;

            var csp = _dbCache.CodedSpareParts.FirstOrDefault(x => string.Equals(x.BarcodeId, barcodeId, StringComparison.OrdinalIgnoreCase));
            if (csp != null) return csp;

            var eq = _dbCache.Equipment.FirstOrDefault(x => string.Equals(x.BarcodeId, barcodeId, StringComparison.OrdinalIgnoreCase));
            if (eq != null) return eq;

            return null;
        }

        public async Task<bool> UpdateItemAsync(BaseItem item)
        {
            if (_dbCache == null) return false;

            if (item is CodedSparePart csp)
            {
                var existing = _dbCache.CodedSpareParts.FirstOrDefault(x => x.BarcodeId == item.BarcodeId);
                if (existing != null)
                {
                    // Map only editable fields
                    existing.PartCode = csp.PartCode;
                    existing.PartName = csp.PartName;
                    existing.Specification = csp.Specification;
                    existing.MinStock = csp.MinStock;
                    existing.Unit = csp.Unit;
                    existing.Note = csp.Note;
                    existing.Supplier = csp.Supplier;
                    existing.Price = csp.Price;
                    existing.Location = csp.Location;
                    existing.Department = csp.Department;
                    return true;
                }
            }
            else if (item is SparePart sp)
            {
                var existing = _dbCache.SpareParts.FirstOrDefault(x => x.BarcodeId == item.BarcodeId);
                if (existing != null)
                {
                    // Map only editable fields
                    existing.PartCode = sp.PartCode;
                    existing.PartName = sp.PartName;
                    existing.Specification = sp.Specification;
                    existing.MinStock = sp.MinStock;
                    existing.Unit = sp.Unit;
                    existing.Note = sp.Note;
                    existing.Supplier = sp.Supplier;
                    existing.Price = sp.Price;
                    existing.Location = sp.Location;
                    existing.Department = sp.Department;
                    return true;
                }
            }

            return false;
        }

        public Task<List<MovementTypeItem>> GetMovementTypesAsync()
        {
            return Task.FromResult(new List<MovementTypeItem>
            {
                new() { MovementTypeID = 1, MovementTypeName = "Maintenance" },
                new() { MovementTypeID = 2, MovementTypeName = "Production" },
                new() { MovementTypeID = 3, MovementTypeName = "Scrap" },
                new() { MovementTypeID = 4, MovementTypeName = "Supplier Return" }
            });
        }

        public Task<ExportOrderResponse> CreateExportOrderAsync(ExportOrderRequest request)
        {
            return Task.FromResult(new ExportOrderResponse
            {
                Success = true,
                Data = new ExportOrderResult
                {
                    ExportID = 1,
                    ExportCode = "EXP-MOCK-001"
                },
                StatusCode = 200
            });
        }

        public Task<List<string>> GetAvailableSerialsAsync(string partCode)
        {
            return Task.FromResult(new List<string>
            {
                $"{partCode}-SN001",
                $"{partCode}-SN002",
                $"{partCode}-SN003"
            });
        }

        private async Task LoadDatabaseAsync()
        {
            try
            {
                // In MAUI, RawAssets are stored in Resources/Raw
                using var stream = await FileSystem.OpenAppPackageFileAsync("database.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _dbCache = JsonSerializer.Deserialize<DatabaseWrapper>(json, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading database.json: {ex.Message}");
            }
        }
    }
}
