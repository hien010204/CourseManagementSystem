using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Models;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public CourseController(ICourseService courseService, IUserService userService)
        {
            _courseService = courseService;
            _userService = userService;
        }

        // API tạo khóa học (Admin và Teacher)
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("create")]
        public IActionResult CreateCourse(
            [FromForm] string courseName,
            [FromForm] string description,
            [FromForm] DateTime startDate,
            [FromForm] DateTime endDate)
        {
            // Lấy ID của người dùng hiện tại từ JWT (thay "IdUser" thành đúng tên claim bạn đã sử dụng khi tạo token)
            var currentUserIdClaim = User.FindFirst("IdUser");

            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            // Kiểm tra quyền
            var currentUser = _userService.GetUserById(currentUserId);
            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Teacher"))
            {
                return Forbid();
            }

            // Chuyển đổi từ DateTime sang DateOnly
            var startDateOnly = DateOnly.FromDateTime(startDate.Date);
            var endDateOnly = DateOnly.FromDateTime(endDate.Date);

            // Tạo khóa học mới
            var newCourse = new Course
            {
                CourseName = courseName,
                Description = description,
                StartDate = startDateOnly,
                EndDate = endDateOnly,
                CreatedBy = currentUserId
            };

            var createdCourse = _courseService.AddCourse(newCourse);
            return Ok(new
            {
                message = "Tạo khóa học thành công!",
                course = createdCourse.CourseName,
                startDate = createdCourse.StartDate,
                endDate = createdCourse.EndDate,
                createdBy = createdCourse.CreatedBy
            });
        }

    }
}
