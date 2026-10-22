using System.Threading.Tasks;

namespace CMMS_QRCode_Scanner.Services
{
    public class MockAuthService : IAuthService
    {
        private bool _isAuthenticated = false;
        public string CurrentUsername { get; private set; }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            // Simulate network delay
            await Task.Delay(1500);

            if (username == "admin" && password == "admin")
            {
                _isAuthenticated = true;
                CurrentUsername = username;
                return new LoginResult(true);
            }

            return new LoginResult(false, "Sai tên đăng nhập hoặc mật khẩu.");
        }

        public void Logout()
        {
            _isAuthenticated = false;
            CurrentUsername = null;
        }

        public bool IsAuthenticated()
        {
            return _isAuthenticated;
        }
    }
}
