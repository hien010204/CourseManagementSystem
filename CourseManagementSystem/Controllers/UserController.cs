using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Email;
using CourseManagementSystem.Services.Profile;
using CourseManagementSystem.Services.Users;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize] // Only allows users who are authenticated via JWT
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProfileService _profileService;
        private readonly IConfiguration _config;
        public UserController(IUserService userService, IConfiguration config, IProfileService profileService)
        {
            _userService = userService;
            _config = config;
            _profileService = profileService;
        }

        // API to add a user (only Admin has permission)
        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public IActionResult AddUser([FromForm] string FullName, [FromForm] string username, [FromForm] string password, [FromForm] string email, [FromForm] string phonenumber, [FromForm] string Status, [FromForm] string role)
        {
            if (_userService.CheckUsernameExists(username))
            {
                return BadRequest(new { message = "Username already exists." });
            }

            // Check if email already exists
            if (_userService.CheckEmailExists(email))
            {
                return BadRequest(new { message = "Email already exists." });
            }
            var user = new User
            {
                UserName = username,
                PasswordHash = password,
                FullName = FullName,
                Email = email,
                PhoneNumber = phonenumber,
                CreatedAt = DateTime.UtcNow,
                Status = Status,
                Role = role  // Default user role
            };
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            var createdUser = _userService.AddUser(user);
            if (createdUser == null)
            {
                return BadRequest(new { message = "Registration failed. The username or email might be taken." });
            }
            // Send email confirmation for successful registration
            try
            {
                // Create email service and configure email to be sent
                var emailService = new EmailService(_config);
                var subject = "Welcome to Cousera - Successful Registration";
                var body = $"Hello {user.UserName},\n\nThank you for registering at Cousera.\n\nWe hope you have a great experience using our service.\n\nBest regards,\nCousera Team";

                // Send confirmation email
                emailService.SendEmail(user.Email, subject, body);
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

        // API to get user information by ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // Lock and Unlock User
        [Authorize(Roles = "Admin")]
        [HttpPut("update-status/{userId}")]
        public IActionResult UpdateUserStatus(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Update the user's status
            user.Status = user.Status == "Active" ? "Inactive" : "Active";  // Toggle from Active to Inactive and vice versa
            user.UpdatedAt = DateTime.UtcNow;
            _userService.UpdateUser(user);

            return Ok(new { message = "User status has been updated.", newStatus = user.Status });
        }

        // Get all users
        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers()
                .Select(user => new
                {
                    user.IdUser,
                    user.UserName,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    user.Status
                }).ToList();

            return Ok(users);
        }

        //iter4

        // tìm kiểu user theo tên, chia ra teacher và student
        [Authorize(Roles = "Admin")]
        [HttpGet("search-user/{fullName}")]
        public IActionResult SearchUserByName(string fullName, string role)
        {
            var user = _userService.GetStudentsByFullName(fullName, role);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }

        // Update User by ID bao gồm tên, email và số điện thoại
        [Authorize(Roles = "Admin")]
        [HttpPut("update-user/{userId}")]
        public IActionResult UpdateUserById(int userId, [FromBody] UpdateProfileRequest request)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Update the profile information
            var result = _profileService.UpdateProfile(user, request.FullName, request.Email, request.PhoneNumber);
            if (!result)
            {
                return BadRequest(new { message = "Profile update failed." });
            }

            return Ok(new { message = "Profile updated successfully." });
        }
        // filter lọc ra danh sách người dùng đang hoạt động theo Role (Student and Teacher)
        [Authorize(Roles = "Admin")]
        [HttpGet("active-users/{role}")]
        public IActionResult GetActiveUsersByRole(string role)
        {
            var activeUsers = _userService.GetUserByStatusAndRole("Active", role);

            if (activeUsers == null || !activeUsers.Any())
            {
                return NotFound(new { message = $"No active {role} users found." });
            }

            return Ok(activeUsers);
        }
        // lấy ra danh sách người dùng đã bị khóa acc theo Role(Student and Teacher)
        [Authorize(Roles = "Admin")]
        [HttpGet("inactive-users/{role}")]
        public IActionResult GetInactiveUsersByRole(string role)
        {
            var inactiveUsers = _userService.GetUserByStatusAndRole("Inactive", role);

            if (inactiveUsers == null || !inactiveUsers.Any())
            {
                return NotFound(new { message = $"No inactive {role} users found." });
            }

            return Ok(inactiveUsers);
        }


    }
}

