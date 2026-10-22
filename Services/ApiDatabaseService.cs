using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using CMMS_QRCode_Scanner.Models;
using Microsoft.Maui.Storage;

namespace CMMS_QRCode_Scanner.Services
{
    // Define the DTO that matches the backend
    public class ScannerItemDto
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string BarcodeId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Serial { get; set; }
        public string? Specification { get; set; }
        public decimal Quantity { get; set; }
        public decimal MinStock { get; set; }
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public string? Location { get; set; }
        public string? Supplier { get; set; }
        public string? Department { get; set; }
        public int AgeDays { get; set; }
        public string? ImportDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class ApiDatabaseService : IMockDatabaseService
    {
        private readonly HttpClient _httpClient;

        public ApiDatabaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task AttachAuthorizationHeaderAsync()
        {
            var token = await SecureStorage.Default.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<BaseItem?> GetItemByBarcodeIdAsync(string barcodeId)
        {
            try
            {
                await AttachAuthorizationHeaderAsync();
                
                var response = await _httpClient.GetAsync($"{AppConfig.BaseApiUrl}Scanner/{barcodeId}");
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ScannerItemDto>();
                    if (dto == null) return null;

                    if (dto.EntityType == "Equipment")
                    {
                        return new Equipment
                        {
                            Id = dto.Id,
                            BarcodeId = dto.BarcodeId,
                            EquipmentCode = dto.Code,
                            EquipmentName = dto.Name,
                            Model = dto.Specification ?? "",
                            Serial = dto.Serial ?? "",
                            Location = dto.Location ?? "",
                            Status = dto.Status ?? "",
                            NextMaintenance = ""
                        };
                    }
                    else if (dto.EntityType == "CodedSparePart")
                    {
                        return new CodedSparePart
                        {
                            Id = dto.Id,
                            BarcodeId = dto.BarcodeId,
                            PartCode = dto.Code,
                            PartName = dto.Name,
                            SerialCode = dto.Serial ?? "",
                            Specification = dto.Specification ?? "",
                            Quantity = (int)dto.Quantity,
                            MinStock = (int)dto.MinStock,
                            Unit = dto.Unit ?? "",
                            Price = dto.Price.ToString("0.##"),
                            Location = dto.Location ?? "",
                            Supplier = dto.Supplier ?? "",
                            Department = dto.Department ?? "",
                            AgeDays = dto.AgeDays,
                            ImportDate = dto.ImportDate ?? "",
                            Status = dto.Status ?? "",
                            Note = dto.Note ?? ""
                        };
                    }
                    else
                    {
                        return new SparePart
                        {
                            Id = dto.Id,
                            BarcodeId = dto.BarcodeId,
                            PartCode = dto.Code,
                            PartName = dto.Name,
                            Specification = dto.Specification ?? "",
                            Quantity = (int)dto.Quantity,
                            MinStock = (int)dto.MinStock,
                            Unit = dto.Unit ?? "",
                            Price = dto.Price.ToString("0.##"),
                            Location = dto.Location ?? "",
                            Supplier = dto.Supplier ?? "",
                            Department = dto.Department ?? "",
                            AgeDays = dto.AgeDays,
                            ImportDate = dto.ImportDate ?? "",
                            Status = dto.Status ?? "",
                            Note = dto.Note ?? ""
                        };
                    }
                }
                else
                {
                    // If not found in Scanner/{barcodeId}, check if barcodeId is a SerialCode in Tbl_SparePartItem
                    var serialResp = await _httpClient.GetAsync($"{AppConfig.BaseApiUrl}SparePart/items?serialCode={Uri.EscapeDataString(barcodeId)}&pageSize=1");
                    if (serialResp.IsSuccessStatusCode)
                    {
                        var paged = await serialResp.Content.ReadFromJsonAsync<PagedResultDto<SparePartItemDto>>();
                        var item = paged?.Items?.FirstOrDefault(i => string.Equals(i.SerialCode, barcodeId, StringComparison.OrdinalIgnoreCase));
                        if (item != null)
                        {
                            return new CodedSparePart
                            {
                                Id = item.SPID,
                                BarcodeId = barcodeId,
                                PartCode = item.PartCode ?? "",
                                PartName = item.PartName ?? "",
                                SerialCode = item.SerialCode ?? barcodeId,
                                Specification = "",
                                Quantity = item.RemainingQuantity > 0 ? item.RemainingQuantity : 1,
                                MinStock = 1,
                                Unit = item.Unit ?? "Cái",
                                Price = (item.Price ?? 0).ToString("0.##"),
                                Location = "",
                                Supplier = item.SupplierName ?? "",
                                Department = "",
                                AgeDays = item.DaysInStock,
                                ImportDate = item.ImportDate.ToString("yyyy-MM-dd"),
                                Status = item.Status ?? "Available",
                                Note = ""
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
            }
            
            return null;
        }

        public async Task<bool> UpdateItemAsync(BaseItem item)
        {
            try
            {
                await AttachAuthorizationHeaderAsync();

                var dto = new ScannerItemDto
                {
                    Id = item.Id,
                    BarcodeId = item.BarcodeId,
                    EntityType = item.EntityType.Contains("Equipment") ? "Equipment" : "SparePart"
                };

                if (item is Equipment eq)
                {
                    dto.Name = eq.EquipmentName;
                    dto.Specification = eq.Model;
                    // Equipment note and price aren't in model but we can ignore it
                }
                else if (item is SparePart sp)
                {
                    dto.Name = sp.PartName;
                    dto.Code = sp.PartCode;
                    dto.Specification = sp.Specification;
                    dto.MinStock = sp.MinStock;
                    dto.Unit = sp.Unit;
                    decimal.TryParse(sp.Price, out var price);
                    dto.Price = price;
                    dto.Note = sp.Note;
                }
                else if (item is CodedSparePart csp)
                {
                    dto.Name = csp.PartName;
                    dto.Code = csp.PartCode;
                    dto.Specification = csp.Specification;
                    dto.MinStock = csp.MinStock;
                    dto.Unit = csp.Unit;
                    decimal.TryParse(csp.Price, out var price);
                    dto.Price = price;
                    dto.Note = csp.Note;
                }

                var response = await _httpClient.PutAsJsonAsync($"{AppConfig.BaseApiUrl}Scanner/update", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return false;
            }
        }
        public async Task<List<MovementTypeItem>> GetMovementTypesAsync()
        {
            try
            {
                await AttachAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<List<MovementTypeItem>>($"{AppConfig.BaseApiUrl}SparePart/movement-types");
                return result ?? new List<MovementTypeItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (GetMovementTypes): {ex.Message}");
                return new List<MovementTypeItem>();
            }
        }

        public async Task<List<string>> GetAvailableSerialsAsync(string partCode)
        {
            try
            {
                await AttachAuthorizationHeaderAsync();
                var url = $"{AppConfig.BaseApiUrl}SparePart/items?partCode={Uri.EscapeDataString(partCode)}&status=Available&pageSize=100";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedResultDto<SparePartItemDto>>();
                    if (result?.Items != null)
                    {
                        return result.Items
                            .Where(i => !string.IsNullOrWhiteSpace(i.SerialCode) && i.RemainingQuantity > 0)
                            .Select(i => i.SerialCode!.Trim())
                            .Distinct()
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (GetAvailableSerials): {ex.Message}");
            }
            return new List<string>();
        }

        public async Task<ExportOrderResponse> CreateExportOrderAsync(ExportOrderRequest request)
        {
            try
            {
                await AttachAuthorizationHeaderAsync();

                // Build the DTO that matches backend ExportOrderDto
                var dto = new
                {
                    MovementTypeID = request.MovementTypeID,
                    ExportDate = request.ExportDate,
                    Details = request.Details.Select(d => new
                    {
                        SPID = d.SPID,
                        PartCode = d.PartCode,
                        PartName = d.PartName,
                        HasCode = d.HasCode,
                        SerialCode = string.IsNullOrWhiteSpace(d.SerialCode) ? null : d.SerialCode,
                        Quantity = d.Quantity,
                        Unit = d.Unit
                    }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync($"{AppConfig.BaseApiUrl}SparePart/export-order", dto);
                var statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ExportOrderResult>();
                    return new ExportOrderResponse
                    {
                        Success = true,
                        Data = result,
                        StatusCode = statusCode
                    };
                }
                else
                {
                    var rawError = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error (CreateExportOrder) [HTTP {statusCode}]: {rawError}");

                    var friendlyMessage = ExtractFriendlyErrorMessage(response.StatusCode, rawError);
                    return new ExportOrderResponse
                    {
                        Success = false,
                        ErrorMessage = friendlyMessage,
                        StatusCode = statusCode
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Network Error (CreateExportOrder): {ex.Message}");
                return new ExportOrderResponse
                {
                    Success = false,
                    ErrorMessage = $"[Lỗi mạng] Không thể kết nối tới server ({AppConfig.BaseApiUrl}).\nChi tiết: {ex.Message}",
                    StatusCode = 0
                };
            }
            catch (TaskCanceledException)
            {
                return new ExportOrderResponse
                {
                    Success = false,
                    ErrorMessage = $"[Timeout] Quá thời gian chờ phản hồi từ máy chủ khi tạo phiếu xuất.",
                    StatusCode = 408
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Exception (CreateExportOrder): {ex.Message}");
                return new ExportOrderResponse
                {
                    Success = false,
                    ErrorMessage = $"[Lỗi ngoại lệ] {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private string ExtractFriendlyErrorMessage(HttpStatusCode statusCode, string rawBody)
        {
            var statusInt = (int)statusCode;

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return "[HTTP 401 Unauthorized] Phiên đăng nhập đã hết hạn hoặc token không hợp lệ. Vui lòng đăng xuất và đăng nhập lại.";
            }

            if (statusCode == HttpStatusCode.Forbidden)
            {
                return "[HTTP 403 Forbidden] Tài khoản của bạn không có quyền tạo phiếu xuất kho.";
            }

            if (statusCode == HttpStatusCode.NotFound)
            {
                return $"[HTTP 404 Not Found] Không tìm thấy API tại endpoint:\n{AppConfig.BaseApiUrl}SparePart/export-order\nVui lòng kiểm tra lại cấu hình.";
            }

            if (string.IsNullOrWhiteSpace(rawBody))
            {
                return $"[HTTP {statusInt}] Máy chủ phản hồi mã lỗi {statusInt} ({statusCode}) nhưng không có nội dung giải thích.";
            }

            // Try to parse JSON response body
            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                // ASP.NET Core ValidationProblemDetails: { "title": "...", "errors": { "Field": ["err1", "err2"] } }
                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errorLines = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                errorLines.Add($"• {prop.Name}: {item.GetString()}");
                            }
                        }
                        else
                        {
                            errorLines.Add($"• {prop.Name}: {prop.Value.GetString()}");
                        }
                    }
                    if (errorLines.Count > 0)
                    {
                        return $"[HTTP {statusInt} Lỗi kiểm tra dữ liệu]\n" + string.Join("\n", errorLines);
                    }
                }

                // Check for "message" field
                if (root.TryGetProperty("message", out var msgProp) && !string.IsNullOrWhiteSpace(msgProp.GetString()))
                {
                    return $"[HTTP {statusInt}] {msgProp.GetString()}";
                }

                // Check for "error" field
                if (root.TryGetProperty("error", out var errProp) && !string.IsNullOrWhiteSpace(errProp.GetString()))
                {
                    return $"[HTTP {statusInt}] {errProp.GetString()}";
                }

                // Check for ProblemDetails "title" and "detail"
                string? title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
                string? detail = root.TryGetProperty("detail", out var detailProp) ? detailProp.GetString() : null;
                if (!string.IsNullOrEmpty(detail) || !string.IsNullOrEmpty(title))
                {
                    return $"[HTTP {statusInt}] {(title != null ? title + ": " : "")}{detail ?? ""}".Trim();
                }
            }
            catch
            {
                // Not JSON, continue to raw string handling
            }

            // If rawBody is HTML (e.g. IIS error page)
            if (rawBody.Contains("<html", StringComparison.OrdinalIgnoreCase))
            {
                var titleMatch = Regex.Match(rawBody, @"<title>(.*?)</title>", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                {
                    return $"[HTTP {statusInt}] Máy chủ IIS: {titleMatch.Groups[1].Value.Trim()}";
                }
                return $"[HTTP {statusInt}] Lỗi máy chủ web IIS.";
            }

            var cleanBody = rawBody.Length > 300 ? rawBody.Substring(0, 300) + "..." : rawBody;
            return $"[HTTP {statusInt}] {cleanBody.Trim()}";
        }
    }
}
