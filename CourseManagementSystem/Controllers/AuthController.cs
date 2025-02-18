using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Email;
using CourseManagementSystem.Services.Users;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public AuthController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string username, [FromForm] string password)
        {
            // Check login information
            var user = _userService.Authenticate(username, password);

            if (user == null)
                return Unauthorized(new { message = "Incorrect username or password." });

            if (user.Status == "Inactive")
            {
                return StatusCode(403, new { message = "Your account has been banned." }); // Return 403 with a message
            }

            // Create JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            // Define claims for the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("IdUser", user.IdUser.ToString()), // Save UserId from the User table
                    new Claim(ClaimTypes.Name, user.UserName),   // Save Username from the User table
                    new Claim(ClaimTypes.Role, user.Role)        // Save the user's role
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token is valid for 1 hour
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Return login information with token and type (role)
            return Ok(new
            {
                IdUser = user.IdUser,
                UserName = user.UserName,
                Role = user.Role,  // Include type in the return info
                Status = user.Status,
                Token = tokenString,
                Expiration = tokenDescriptor.Expires
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromForm] string FullName, [FromForm] string username, [FromForm] string email, [FromForm] string password, [FromForm] string phonenumber)
        {
            // Check if username already exists
            if (_userService.CheckUsernameExists(username))
            {
                return BadRequest(new { message = "Username already exists." });
            }

            // Check if email already exists
            if (_userService.CheckUsernameExists(email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            // Create a new user with hashed password
            var newUser = new User
            {
                UserName = username,
                PasswordHash = password, // Hash password
                FullName = FullName,
                Email = email,
                PhoneNumber = phonenumber,
                CreatedAt = DateTime.UtcNow,
                Role = "Student"  // Default user role
            };

            var createdUser = _userService.Register(newUser);

            if (createdUser == null)
            {
                return BadRequest(new { message = "Registration failed." });
            }

            // Send email confirmation for successful registration
            try
            {
                // Create email service and configure email to be sent
                var emailService = new EmailService(_config);
                var subject = "Welcome to Cousera - Successful Registration";
                var body = $"Hello {newUser.UserName},\n\nThank you for registering at Cousera.\n\nWe hope you have a great experience using our service.\n\nBest regards,\nCousera Team";

                // Send confirmation email
                emailService.SendEmail(newUser.Email, subject, body);
            }
            catch (SmtpCommandException smtpEx)
            {
                // If there is an SMTP error (e.g., connection failure or authentication error)
                // Provide clearer error message
                return BadRequest(new { message = "Error connecting to the email service. Please try again later." });
            }
            catch (Exception ex)
            {
                // Handle general errors (undetermined cause)
                Console.WriteLine($"General Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);  // Provide detailed error information
                return BadRequest(new { message = "Failed to send confirmation email.", errorDetails = ex.Message });
            }

            return Ok(new
            {
                FullName = createdUser.FullName,
                Username = createdUser.UserName,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _userService.Logout();
            return Ok(new { message = "Successfully logged out." });
        }



        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromForm] string usernameOrEmail)
        {
            // Check if the user has registered with the provided username or email
            var user = _userService.GetUserByUsernameOrEmail(usernameOrEmail);

            if (user == null)
            {
                return NotFound(new { message = "No account found with this username or email." });
            }

            // Create a verification code or reset password link
            var verificationCode = Guid.NewGuid().ToString(); // Generate a random verification code

            // Save verification code to database or send via email
            // Assume we send the code via email
            try
            {
                // Create email service and configure email to be sent
                var emailService = new EmailService(_config);
                var subject = "Password Change Request";
                var body = $"Hello {user.UserName},\n\nYou requested to change your password.\n\nUse the following verification code to create a new password: {verificationCode}\n\nIf you did not request a password change, please ignore this email.\n\nBest regards,\nSupport Team";

                // Send verification email
                emailService.SendEmail(user.Email, subject, body);

                // Save verification code to database for later comparison when user provides it
                _userService.SavePasswordResetCode(user.IdUser, verificationCode);
            }
            catch (SmtpCommandException smtpEx)
            {
                return BadRequest(new { message = "Error connecting to the email service. Please try again later." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to send email.", errorDetails = ex.Message });
            }

            return Ok(new { message = "The verification code has been sent to your email. Please check your email to change the password." });
        }

        [HttpPut("reset-password")]
        public IActionResult ResetPassword([FromForm] string usernameOrEmail, [FromForm] string verificationCode, [FromForm] string newPassword)
        {
            // Check the user and verification code
            var user = _userService.GetUserByUsernameOrEmail(usernameOrEmail);
            if (user == null)
            {
                return NotFound(new { message = "No account found with this username or email." });
            }

            // Check verification code
            var storedVerificationCode = _userService.GetPasswordResetCode(user.IdUser);
            if (storedVerificationCode != verificationCode)
            {
                return BadRequest(new { message = "Invalid verification code." });
            }

            // Update user's password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            _userService.ChangePassword(user);

            return Ok(new { message = "Your password has been successfully changed." });
        }
    }
}
