using System.Threading.Tasks;

namespace CMMS_QRCode_Scanner.Services
{
    public record LoginResult(bool Success, string? ErrorMessage = null);

    public interface IAuthService
    {
        string CurrentUsername { get; }
        Task<LoginResult> LoginAsync(string username, string password);
        void Logout();
        bool IsAuthenticated();
    }
}
