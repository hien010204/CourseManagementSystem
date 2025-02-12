using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize] // Chỉ cho phép người dùng đã xác thực qua JWT
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // API để thêm người dùng (chỉ Admin có quyền)
        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public IActionResult AddUser([FromForm] string FullName, [FromForm] string username, [FromForm] string password, [FromForm] string email, [FromForm] string phonenumber, [FromForm] string Status, [FromForm] string role)
        {
            var user = new User
            {
                UserName = username,
                PasswordHash = password,
                FullName = FullName,
                Email = email,
                PhoneNumber = phonenumber,
                CreatedAt = DateTime.UtcNow,
                Status = Status,
                Role = role  // Loại người dùng mặc định
            };
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            var createdUser = _userService.AddUser(user);
            return BadRequest(new { message = "Đăng ký không thành công." });
        }


        // API để lấy thông tin người dùng theo ID
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

        //Lock and Unlock User
        [Authorize(Roles = "Admin")]
        [HttpPut("update-status/{userId}")]
        public IActionResult UpdateUserStatus(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Cập nhật trạng thái của người dùng
            user.Status = user.Status == "Active" ? "Inactive" : "Active";  // Đổi từ Active thành Inactive và ngược lại
            user.UpdatedAt = DateTime.UtcNow;
            _userService.UpdateUser(user);

            return Ok(new { message = "Trạng thái của người dùng đã được cập nhật.", newStatus = user.Status });

        }
        //get all users
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
    }

}
