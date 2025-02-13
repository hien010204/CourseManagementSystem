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

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPut("edit/{courseId}")]
        public IActionResult EditCourse(
            [FromRoute] int courseId,
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

            // Tìm khóa học cần chỉnh sửa
            var courseToEdit = _courseService.GetCourseById(courseId);
            if (courseToEdit == null)
            {
                return NotFound(new { message = "Khóa học không tồn tại." });
            }

            // Chỉnh sửa thông tin khóa học
            courseToEdit.CourseName = courseName;
            courseToEdit.Description = description;
            courseToEdit.StartDate = DateOnly.FromDateTime(startDate.Date);
            courseToEdit.EndDate = DateOnly.FromDateTime(endDate.Date);

            var updatedCourse = _courseService.EditCourse(courseToEdit);

            return Ok(new
            {
                message = "Chỉnh sửa khóa học thành công!",
                course = updatedCourse.CourseName,
                startDate = updatedCourse.StartDate,
                endDate = updatedCourse.EndDate
            });
        }


        [HttpGet("information/{courseId}")]
        public IActionResult GetCourseById([FromRoute] int courseId)
        {
            // Lấy thông tin khóa học từ dịch vụ
            var course = _courseService.GetCourseById(courseId);
            if (course == null)
            {
                return NotFound(new { message = "Khóa học không tồn tại." });
            }

            return Ok(new
            {
                course.CourseName,
                course.Description,
                startDate = course.StartDate,
                endDate = course.EndDate,
                createdBy = course.CreatedBy
            });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("all-courses")]
        public IActionResult GetAllCourses()
        {
            var courses = _courseService.GetAllCourses();
            if (courses == null || !courses.Any())
            {
                return NotFound(new { message = "Không có khóa học nào." });
            }

            var courseList = courses.Select(course => new
            {
                course.CourseId,
                course.CourseName,
                course.Description,
                startDate = course.StartDate,
                endDate = course.EndDate
            }).ToList();

            return Ok(courseList);

        }

        // API xóa khóa học (Admin có thể xóa khóa học)
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{courseId}")]
        public IActionResult DeleteCourse([FromRoute] int courseId)
        {
            // Kiểm tra sự tồn tại của khóa học
            var courseToDelete = _courseService.GetCourseById(courseId);
            if (courseToDelete == null)
            {
                return NotFound(new { message = "Khóa học không tồn tại." });
            }

            // Xóa khóa học
            var isDeleted = _courseService.DeleteCourse(courseId);
            if (!isDeleted)
            {
                return BadRequest(new { message = "Xóa khóa học không thành công." });
            }

            return Ok(new { message = "Xóa khóa học thành công!" });
        }


        [Authorize]
        [HttpGet("my-courses")]
        public IActionResult GetUserCourses()
        {
            // Lấy ID người dùng từ JWT
            var currentUserIdClaim = User.FindFirst("IdUser");
            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            // Lấy các khóa học mà người dùng đã đăng ký từ service
            var courseList = _courseService.GetUserCourses(currentUserId);

            if (courseList == null || !courseList.Any())
            {
                return NotFound(new { message = "Người dùng chưa đăng ký khóa học nào." });
            }

            return Ok(courseList);
        }



        [Authorize]
        [HttpPost("enroll/{courseId}")]
        public IActionResult EnrollInCourse([FromRoute] int courseId)
        {
            // Lấy ID người dùng từ JWT
            var currentUserIdClaim = User.FindFirst("IdUser");
            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            // Gọi phương thức EnrollInCourse trong CourseService để đăng ký
            var isEnrolled = _courseService.EnrollInCourse(courseId, currentUserId);

            if (!isEnrolled)
            {
                return BadRequest(new { message = "Không thể đăng ký khóa học (khóa học không tồn tại hoặc bạn đã đăng ký rồi)." });
            }

            return Ok(new { message = "Đăng ký khóa học thành công!" });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("confirm-enrollment/{courseId}/{studentId}")]
        public IActionResult ConfirmEnrollment(int courseId, int studentId)
        {
            // Kiểm tra quyền của người dùng (Admin hoặc Teacher)
            var currentUserIdClaim = User.FindFirst("IdUser");
            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);
            var currentUser = _userService.GetUserById(currentUserId);

            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Teacher"))
            {
                return Forbid(); // Nếu người dùng không phải Admin hoặc Teacher
            }

            // Gọi service để xác nhận đăng ký
            var result = _courseService.ConfirmEnrollment(courseId, studentId);

            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy đăng ký nào với trạng thái Pending." });
            }

            return Ok(new { message = "Đăng ký học sinh vào khóa học đã được xác nhận." });
        }


    }
}
