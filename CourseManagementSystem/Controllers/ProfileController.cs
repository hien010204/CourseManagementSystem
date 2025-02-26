using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Profile;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IUserService _userService;

        public ProfileController(IProfileService profileService, IUserService userService)
        {
            _profileService = profileService;
            _userService = userService;
        }


        [HttpGet("get-profile")]
        public IActionResult GetProfile()
        {
            // Get user ID from the claims and parse it into an integer
            var currentUserIdClaim = User.FindFirst("IdUser")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return NotFound(new { message = "User not found." });
            }

            // Fetch the user details using the current user ID
            var user = _userService.GetUserById(currentUserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status,
                Role = user.Role
            });
        }


        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            // Get user ID from the claims and parse it into an integer
            var currentUserIdClaim = User.FindFirst("IdUser")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return NotFound(new { message = "User not found." });
            }

            // Fetch the user details using the current user ID
            var user = _userService.GetUserById(currentUserId);
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


        [HttpPut("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Get user ID from the claims and parse it into an integer
            var currentUserIdClaim = User.FindFirst("IdUser")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return NotFound(new { message = "User not found." });
            }

            // Change the password
            var result = _profileService.ChangePassword(currentUserId, request.CurrentPassword, request.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Invalid current password or password change failed." });
            }

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
