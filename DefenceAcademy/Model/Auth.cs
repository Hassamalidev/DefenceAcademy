using System.ComponentModel.DataAnnotations;

namespace DefenceAcademy.Model
{
    public enum RegistrationErrorType
    {
        None = 0,
        UsernameExists = 1,
        EmailExists = 2,
        WeakPassword = 3,
        InvalidEmail = 4,
        InvalidData = 5,
        DatabaseError = 6
    }

    public enum LoginErrorType
    {
        None = 0,
        InvalidCredentials = 1,
        UserNotFound = 2,
        InactiveAccount = 3,
        AdminNotApproved = 4,
        DatabaseError = 5
    }

    public class RegistrationResult
    {
        public bool Success { get; set; }
        public RegistrationErrorType? ErrorType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public LoginErrorType? ErrorType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public LoginResponse? Response { get; set; }
    }

    public class Auth
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalToken { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}