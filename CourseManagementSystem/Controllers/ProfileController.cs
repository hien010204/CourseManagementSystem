using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Profile;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IUserService _userService;

        public ProfileController(IProfileService profileService, IUserService userService)
        {
            _profileService = profileService;
            _userService = userService;
        }

        // API lấy thông tin profile của người dùng
        [HttpGet("{userId}/get-profile")]
        public IActionResult GetProfile(int userId)
        {
            var user = _userService.GetUserById(userId);
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

        // API cập nhật thông tin profile của người dùng
        [HttpPut("{userId}/update-profile")]
        public IActionResult UpdateProfile(int userId, [FromBody] UpdateProfileRequest request)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var result = _profileService.UpdateProfile(user, request.FullName, request.Email, request.PhoneNumber);
            if (!result)
            {
                return BadRequest(new { message = "Profile update failed." });
            }

            return Ok(new { message = "Profile updated successfully." });
        }

        // API thay đổi mật khẩu
        [HttpPut("{userId}/change-password")]
        public IActionResult ChangePassword(int userId, [FromBody] ChangePasswordRequest request)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var result = _profileService.ChangePassword(user, request.CurrentPassword, request.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Invalid current password or password change failed." });
            }

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
