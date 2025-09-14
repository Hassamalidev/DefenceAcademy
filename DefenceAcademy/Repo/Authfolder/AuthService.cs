using Dapper;
using DefenceAcademy.Model;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace DefenceAcademy.Repo.Authfolder
{
    public class AuthService : IAuthService
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpirationHours;

        public AuthService(DapperContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

            _jwtSecret = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Secret not configured");
            _jwtIssuer = _configuration["Jwt:Issuer"] ?? "DefenceAcademy";
            _jwtAudience = _configuration["Jwt:Audience"] ?? "DefenceAcademy";

            if (!int.TryParse(_configuration["Jwt:ExpirationHours"] ?? "24", out _jwtExpirationHours))
            {
                _jwtExpirationHours = _configuration.GetValue<int>("Jwt:ExpirationHours", 24);
            }
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with invalid credentials - Email: {Email}", request?.Email ?? "null");
                return new LoginResult
                {
                    Success = false,
                    ErrorType = LoginErrorType.InvalidCredentials,
                    ErrorMessage = "Email and password are required"
                };
            }

            try
            {
                _logger.LogInformation("Starting login process for email: {Email}", request.Email);
                var user = await GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found with email: {Email}", request.Email);
                    return new LoginResult
                    {
                        Success = false,
                        ErrorType = LoginErrorType.UserNotFound,
                        ErrorMessage = "User not found"
                    };
                }

                _logger.LogInformation("User found: {Email}, IsActive: {IsActive}, Role: {Role}, IsApproved: {IsApproved}",
                    user.Email, user.IsActive, user.Role, user.IsApproved);

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user login attempt: {Email}", request.Email);
                    return new LoginResult
                    {
                        Success = false,
                        ErrorType = LoginErrorType.InactiveAccount,
                        ErrorMessage = "Account is inactive"
                    };
                }

                var passwordValid = VerifyPassword(request.Password, user.PasswordHash);
                _logger.LogInformation("Password verification result for {Email}: {IsValid}", request.Email, passwordValid);

                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password attempt for user: {Email}", request.Email);
                    return new LoginResult
                    {
                        Success = false,
                        ErrorType = LoginErrorType.InvalidCredentials,
                        ErrorMessage = "Invalid password"
                    };
                }

                if (user.Role == "Admin" && !user.IsApproved)
                {
                    _logger.LogWarning("Admin login attempt before approval: {Email}", request.Email);
                    return new LoginResult
                    {
                        Success = false,
                        ErrorType = LoginErrorType.AdminNotApproved,
                        ErrorMessage = "Admin account pending approval"
                    };
                }

                var token = GenerateJwtToken(user);
                var expirationTime = DateTime.UtcNow.AddHours(_jwtExpirationHours);

                _logger.LogInformation("Login successful for user: {Email}", request.Email);

                return new LoginResult
                {
                    Success = true,
                    Response = new LoginResponse
                    {
                        Token = token,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        ExpiresAt = expirationTime
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new LoginResult
                {
                    Success = false,
                    ErrorType = LoginErrorType.DatabaseError,
                    ErrorMessage = "An error occurred during login"
                };
            }
        }

        public async Task<RegistrationResult> RegisterAsync(RegisterRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Registration attempt with invalid data");
                return new RegistrationResult
                {
                    Success = false,
                    ErrorType = RegistrationErrorType.InvalidData,
                    ErrorMessage = "All fields are required"
                };
            }

            if (!IsValidEmail(request.Email))
            {
                _logger.LogWarning("Registration attempt with invalid email: {Email}", request.Email);
                return new RegistrationResult
                {
                    Success = false,
                    ErrorType = RegistrationErrorType.InvalidEmail,
                    ErrorMessage = "Invalid email format"
                };
            }

            try
            {
                var existingUser = await GetUserByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
                    return new RegistrationResult
                    {
                        Success = false,
                        ErrorType = RegistrationErrorType.UsernameExists,
                        ErrorMessage = "Username already exists"
                    };
                }

                var existingEmail = await GetUserByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return new RegistrationResult
                    {
                        Success = false,
                        ErrorType = RegistrationErrorType.EmailExists,
                        ErrorMessage = "Email already registered"
                    };
                }

                var passwordValidation = ValidatePasswordStrength(request.Password);
                if (!passwordValidation.IsValid)
                {
                    _logger.LogWarning("Registration attempt with weak password for user: {Username}", request.Username);
                    return new RegistrationResult
                    {
                        Success = false,
                        ErrorType = RegistrationErrorType.WeakPassword,
                        ErrorMessage = passwordValidation.ErrorMessage
                    };
                }

                var passwordHash = HashPassword(request.Password);
                var approvalToken = request.Role == "Admin" ? GenerateSecureToken() : null;
                var isApproved = request.Role != "Admin";

                var sql = @"INSERT INTO Users (Username, Email, PasswordHash, Role, IsApproved, IsActive, CreatedAt, ApprovalToken) 
                           VALUES (@Username, @Email, @PasswordHash, @Role, @IsApproved, @IsActive, @CreatedAt, @ApprovalToken)";

                var user = new
                {
                    Username = request.Username.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = passwordHash,
                    request.Role,
                    IsApproved = isApproved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ApprovalToken = approvalToken
                };

                using (var connection = await _context.createConnection())
                {
                    var result = await connection.ExecuteAsync(sql, user);

                    if (result > 0)
                    {
                        if (request.Role == "Admin")
                        {
                            await SendAdminApprovalEmailAsync(request.Email, request.Username, approvalToken!);
                            return new RegistrationResult
                            {
                                Success = true,
                                RequiresApproval = true
                            };
                        }

                        return new RegistrationResult
                        {
                            Success = true,
                            RequiresApproval = false
                        };
                    }

                    return new RegistrationResult
                    {
                        Success = false,
                        ErrorType = RegistrationErrorType.DatabaseError,
                        ErrorMessage = "Failed to create user account"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
                return new RegistrationResult
                {
                    Success = false,
                    ErrorType = RegistrationErrorType.DatabaseError,
                    ErrorMessage = "An error occurred during registration"
                };
            }
        }

        public async Task<Auth?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            _logger.LogInformation("Checking username: '{Username}'", username.Trim());

            var sql = "SELECT * FROM Users WHERE Username = @Username";
            using (var connection = await _context.createConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<Auth>(sql, new { Username = username.Trim() });
                _logger.LogInformation("Username check result: {Result}", result != null ? "FOUND" : "NOT FOUND");
                return result;
            }
        }

        public async Task<Auth?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                return null;

            _logger.LogInformation("Checking email: '{Email}'", email.Trim().ToLower());

            var sql = "SELECT * FROM Users WHERE Email = @Email";
            using (var connection = await _context.createConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<Auth>(sql, new { Email = email.Trim().ToLower() });
                _logger.LogInformation("Email check result: {Result}", result != null ? "FOUND" : "NOT FOUND");
                return result;
            }
        }

        public async Task<Auth?> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var sql = "SELECT * FROM Users WHERE Id = @Id";
            using (var connection = await _context.createConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Auth>(sql, new { Id = id });
            }
        }

        public async Task<bool> ApproveAdminAsync(string token, bool approved)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var user = await GetPendingAdminByTokenAsync(token);
                if (user == null)
                {
                    _logger.LogWarning("Approval attempt with invalid token: {Token}", token);
                    return false;
                }

                string sql;
                if (approved)
                {
                    sql = @"UPDATE Users SET IsApproved = 1, ApprovedAt = @ApprovedAt, ApprovalToken = NULL 
                           WHERE ApprovalToken = @Token";

                    using (var connection = await _context.createConnection())
                    {
                        var result = await connection.ExecuteAsync(sql, new
                        {
                            ApprovedAt = DateTime.UtcNow,
                            Token = token
                        });

                        if (result > 0)
                        {
                            _logger.LogInformation("Admin approved: {Username}", user.Username);
                            await SendAdminApprovalResultEmailAsync(user.Email, user.Username, true);
                        }

                        return result > 0;
                    }
                }
                else
                {
                    sql = "DELETE FROM Users WHERE ApprovalToken = @Token";
                    using (var connection = await _context.createConnection())
                    {
                        var result = await connection.ExecuteAsync(sql, new { Token = token });

                        if (result > 0)
                        {
                            _logger.LogInformation("Admin registration rejected: {Username}", user.Username);
                            await SendAdminApprovalResultEmailAsync(user.Email, user.Username, false);
                        }

                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin approval with token: {Token}", token);
                return false;
            }
        }

        public async Task<Auth?> GetPendingAdminByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var sql = "SELECT * FROM Users WHERE ApprovalToken = @Token AND Role = 'Admin' AND IsApproved = 0";
            using (var connection = await _context.createConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Auth>(sql, new { Token = token });
            }
        }

        public async Task<IEnumerable<Auth>> GetPendingAdminsAsync()
        {
            try
            {
                var sql = "SELECT * FROM Users WHERE Role = 'Admin' AND IsApproved = 0 ORDER BY CreatedAt DESC";
                using (var connection = await _context.createConnection())
                {
                    return await connection.QueryAsync<Auth>(sql);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending admins");
                return Enumerable.Empty<Auth>();
            }
        }

        private string GenerateJwtToken(Auth user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("username", user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtExpirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            byte[] salt;
            using (var rng = RandomNumberGenerator.Create())
            {
                salt = new byte[16];
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(32);
                var combined = new byte[48];
                Array.Copy(salt, 0, combined, 0, 16);
                Array.Copy(hash, 0, combined, 16, 32);
                return Convert.ToBase64String(combined);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var combined = Convert.FromBase64String(storedHash);
                var salt = new byte[16];
                var hash = new byte[32];
                Array.Copy(combined, 0, salt, 0, 16);
                Array.Copy(combined, 16, hash, 0, 32);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
                {
                    var computedHash = pbkdf2.GetBytes(32);
                    return CryptographicOperations.FixedTimeEquals(hash, computedHash);
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task SendAdminApprovalEmailAsync(string adminEmail, string username, string approvalToken)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = smtpSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var senderEmail = smtpSettings["SenderEmail"] ?? "";
                var senderPassword = smtpSettings["SenderPassword"] ?? "";
                var adminApprovalEmail = smtpSettings["AdminApprovalEmail"] ?? senderEmail;

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogWarning("Email configuration is missing");
                    return;
                }

                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
                var approveUrl = $"{baseUrl}/api/auth/approve-admin?token={approvalToken}&approved=true";
                var rejectUrl = $"{baseUrl}/api/auth/approve-admin?token={approvalToken}&approved=false";

                var subject = "New Admin Registration Approval Required - Defence Academy";
                var body = $@"
                    <h2>New Admin Registration Request</h2>
                    <p>A new admin account has been registered and requires your approval:</p>
                    <ul>
                        <li><strong>Username:</strong> {username}</li>
                        <li><strong>Email:</strong> {adminEmail}</li>
                        <li><strong>Registration Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                    </ul>
                    <p>Please click one of the following links to approve or reject this registration:</p>
                    <p>
                        <a href='{approveUrl}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-right: 10px;'>APPROVE</a>
                        <a href='{rejectUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>REJECT</a>
                    </p>
                    <p><small>If the links don't work, copy and paste them into your browser address bar.</small></p>
                ";

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    var message = new MailMessage(senderEmail, adminApprovalEmail, subject, body)
                    {
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message);
                    _logger.LogInformation("Admin approval email sent for user: {Username}", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval email for user: {Username}", username);
            }
        }

        private async Task SendAdminApprovalResultEmailAsync(string userEmail, string username, bool approved)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = smtpSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var senderEmail = smtpSettings["SenderEmail"] ?? "";
                var senderPassword = smtpSettings["SenderPassword"] ?? "";

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogWarning("Email configuration is missing");
                    return;
                }

                var subject = approved ?
                    "Admin Registration Approved - Defence Academy" :
                    "Admin Registration Rejected - Defence Academy";

                var body = approved ?
                    $@"
                        <h2>Admin Registration Approved</h2>
                        <p>Your admin account registration has been approved.</p>
                        <p>You can now login to your account using your credentials.</p>
                        <p><strong>Username:</strong> {username}</p>
                    " :
                    $@"
                        <h2>Admin Registration Rejected</h2>
                        <p>Your admin account registration has been rejected.</p>
                        <p>If you believe this is an error, please contact the system administrator.</p>
                    ";

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    var message = new MailMessage(senderEmail, userEmail, subject, body)
                    {
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message);
                    _logger.LogInformation("Admin approval result email sent to: {Email}", userEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval result email to: {Email}", userEmail);
            }
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long");

            var hasUpperCase = false;
            var hasLowerCase = false;
            var hasDigit = false;
            var hasSpecialChar = false;
            var specialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            foreach (var c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                else if (char.IsLower(c)) hasLowerCase = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (specialCharacters.Contains(c)) hasSpecialChar = true;
            }

            var errors = new List<string>();

            if (!hasUpperCase)
                errors.Add("at least one uppercase letter");
            if (!hasLowerCase)
                errors.Add("at least one lowercase letter");
            if (!hasDigit)
                errors.Add("at least one digit");
            if (!hasSpecialChar)
                errors.Add("at least one special character");

            if (errors.Count > 0)
                return (false, $"Password must contain {string.Join(", ", errors)}");

            return (true, string.Empty);
        }
    }
}