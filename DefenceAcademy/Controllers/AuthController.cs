using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DefenceAcademy.Model;
using DefenceAcademy.Repo.Authfolder;

namespace DefenceAcademy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.LoginAsync(request);
            if (response == null)
                return Unauthorized("Invalid credentials or account not approved");

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Role != "User" && request.Role != "Admin")
                return BadRequest("Role must be either 'User' or 'Admin'");

            var result = await _authService.RegisterAsync(request);
            if (!result)
                return BadRequest("Username or email already exists");

            if (request.Role == "Admin")
            {
                return Ok(new
                {
                    message = "Admin registration submitted. Please wait for approval via email.",
                    requiresApproval = true
                });
            }

            return Ok(new
            {
                message = "User registered successfully",
                requiresApproval = false
            });
        }

        [HttpGet("approve-admin")]
        public async Task<ActionResult> ApproveAdmin([FromQuery] string token, [FromQuery] bool approved)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid approval token");

            var user = await _authService.GetPendingAdminByTokenAsync(token);
            if (user == null)
                return NotFound("Invalid or expired approval token");

            var result = await _authService.ApproveAdminAsync(token, approved);
            if (!result)
                return BadRequest("Failed to process approval");

            var action = approved ? "approved" : "rejected";
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Admin Approval - Defence Academy</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 50px; text-align: center; }}
                        .success {{ color: #28a745; }}
                        .danger {{ color: #dc3545; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Defence Academy - Admin Approval</h2>
                        <div class='{(approved ? "success" : "danger")}'>
                            <h3>Admin account for '{user.Username}' has been {action}!</h3>
                            <p>Email: {user.Email}</p>
                            {(approved ? "<p>The user can now login with admin privileges.</p>" : "<p>The registration has been rejected and removed.</p>")}
                        </div>
                    </div>
                </body>
                </html>";

            return Content(html, "text/html");
        }

        [HttpGet("pending-admins")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetPendingAdmins()
        {
            var pendingAdmins = await _authService.GetPendingAdminsAsync();
            return Ok(pendingAdmins);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult> GetProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.CreatedAt,
                user.IsActive
            });
        }
    }
}

