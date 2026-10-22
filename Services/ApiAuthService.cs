using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace CMMS_QRCode_Scanner.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private string _currentUsername;

        public string CurrentUsername => _currentUsername;

        public ApiAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            // Try to load username from storage
            var savedUser = Task.Run(() => SecureStorage.Default.GetAsync("current_fullname")).Result;
            if (string.IsNullOrEmpty(savedUser))
                savedUser = Task.Run(() => SecureStorage.Default.GetAsync("current_username")).Result;

            if (!string.IsNullOrEmpty(savedUser))
            {
                _currentUsername = savedUser;
            }
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var url = $"{AppConfig.BaseApiUrl}Auth/login";
            try
            {
                var requestBody = new { UserName = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    // Backend returns: { "token": "..." }
                    var result = await response.Content.ReadFromJsonAsync<System.Collections.Generic.Dictionary<string, string>>();
                    if (result != null && result.ContainsKey("token"))
                    {
                        var token = result["token"];
                        var fullName = GetFullNameFromJwt(token);
                        var displayName = string.IsNullOrEmpty(fullName) ? username : fullName;

                        await SecureStorage.Default.SetAsync("jwt_token", token);
                        await SecureStorage.Default.SetAsync("current_username", username);
                        await SecureStorage.Default.SetAsync("current_fullname", displayName);
                        _currentUsername = displayName;
                        return new LoginResult(true);
                    }

                    return new LoginResult(false, "Phản hồi từ server không chứa token xác thực.");
                }

                // Differentiate HTTP error status codes
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new LoginResult(false, "Sai tên đăng nhập hoặc mật khẩu (401 Unauthorized).");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new LoginResult(false, $"Lỗi 404 Not Found: Không tìm thấy API tại URL:\n{url}\nVui lòng kiểm tra lại cấu hình BaseApiUrl.");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return new LoginResult(false, "Lỗi máy chủ 500 (Internal Server Error): Máy chủ backend hoặc kết nối Database bị lỗi.");
                }

                return new LoginResult(false, $"Lỗi máy chủ: {(int)response.StatusCode} {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Login Error] {ex}");

                var exStr = ex.ToString();
                if (exStr.Contains("Cleartext", StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginResult(false, "Lỗi Android: Kết nối HTTP không bảo mật bị chặn (Cleartext Traffic).\nCần thêm android:usesCleartextTraffic=\"true\" vào AndroidManifest.xml.");
                }
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    return new LoginResult(false, $"Hết thời gian chờ (Timeout) khi kết nối tới:\n{url}\nVui lòng kiểm tra server có đang chạy và mở cổng không.");
                }
                if (ex is HttpRequestException)
                {
                    return new LoginResult(false, $"Không thể kết nối tới server ({ex.InnerException?.Message ?? ex.Message}).\nURL: {url}");
                }

                return new LoginResult(false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public void Logout()
        {
            SecureStorage.Default.Remove("jwt_token");
            SecureStorage.Default.Remove("current_username");
            SecureStorage.Default.Remove("current_fullname");
            _currentUsername = string.Empty;
        }

        private string GetFullNameFromJwt(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length > 1)
                {
                    var payload = parts[1];
                    switch (payload.Length % 4)
                    {
                        case 2: payload += "=="; break;
                        case 3: payload += "="; break;
                    }
                    var jsonBytes = Convert.FromBase64String(payload);
                    var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                    var dict = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json);
                    
                    var nameClaimUri = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
                    if (dict != null)
                    {
                        if (dict.ContainsKey(nameClaimUri)) return dict[nameClaimUri]?.ToString() ?? "";
                        if (dict.ContainsKey("unique_name")) return dict["unique_name"]?.ToString() ?? "";
                        if (dict.ContainsKey("name")) return dict["name"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        public bool IsAuthenticated()
        {
            var token = Task.Run(() => SecureStorage.Default.GetAsync("jwt_token")).Result;
            return !string.IsNullOrEmpty(token);
        }
    }
}
