﻿using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("api/[controller]")]
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

        // API để lấy tất cả người dùng
        [HttpGet]
        public ActionResult<List<User>> GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }
    }

}
