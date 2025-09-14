using DefenceAcademy.Model;

namespace DefenceAcademy.Repo.Authfolder
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<RegistrationResult> RegisterAsync(RegisterRequest request);
        Task<Auth?> GetUserByUsernameAsync(string username);
        Task<Auth?> GetUserByEmailAsync(string email);
        Task<Auth?> GetUserByIdAsync(int id);
        Task<bool> ApproveAdminAsync(string token, bool approved);
        Task<Auth?> GetPendingAdminByTokenAsync(string token);
        Task<IEnumerable<Auth>> GetPendingAdminsAsync();
    }
}