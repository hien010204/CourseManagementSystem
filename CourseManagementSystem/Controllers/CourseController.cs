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

        // API to create a course (Admin and Teacher)
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("create")]
        public IActionResult CreateCourse(
            [FromForm] string courseName,
            [FromForm] string description,
            [FromForm] DateTime startDate,
            [FromForm] DateTime endDate)
        {
            var currentUserIdClaim = User.FindFirst("IdUser");

            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "User information not found in token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            // Check permissions
            var currentUser = _userService.GetUserById(currentUserId);
            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Teacher"))
            {
                return Forbid();
            }

            var startDateOnly = DateOnly.FromDateTime(startDate.Date);
            var endDateOnly = DateOnly.FromDateTime(endDate.Date);

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
                message = "Course created successfully!",

                courseName = createdCourse.CourseName,
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
            var currentUserIdClaim = User.FindFirst("IdUser");

            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "User information not found in token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            // Check permissions
            var currentUser = _userService.GetUserById(currentUserId);
            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Teacher"))
            {
                return Forbid();
            }

            var courseToEdit = _courseService.GetCourseById(courseId);
            if (courseToEdit == null)
            {
                return NotFound(new { message = "Course does not exist." });
            }

            courseToEdit.CourseName = courseName;
            courseToEdit.Description = description;
            courseToEdit.StartDate = DateOnly.FromDateTime(startDate.Date);
            courseToEdit.EndDate = DateOnly.FromDateTime(endDate.Date);

            var updatedCourse = _courseService.EditCourse(courseToEdit);

            return Ok(new
            {
                message = "Course edited successfully!",
                course = updatedCourse.CourseName,
                startDate = updatedCourse.StartDate,
                endDate = updatedCourse.EndDate
            });
        }


        [HttpGet("information/{courseId}")]
        public IActionResult GetCourseById([FromRoute] int courseId)
        {
            var course = _courseService.GetCourseById(courseId);
            var teacher = _courseService.GetTeacherByCourseId(courseId);
            if (course == null)
            {
                return NotFound(new { message = "Course does not exist." });
            }

            // Kiểm tra xem giảng viên có tồn tại không trước khi hiển thị
            if (teacher == null)
            {
                return Ok(new
                {
                    course.CourseId,
                    course.CourseName,
                    course.Description,
                    startDate = course.StartDate,
                    endDate = course.EndDate,
                    createdBy = new
                    {
                        course.CreatedByNavigation.FullName,
                        course.CreatedByNavigation.Role
                    },
                    teacher = "No teacher assigned" // Thêm thông báo nếu không có giảng viên
                });
            }

            return Ok(new
            {
                course.CourseId,
                course.CourseName,
                course.Description,
                startDate = course.StartDate,
                endDate = course.EndDate,
                createdBy = new
                {
                    course.CreatedByNavigation.FullName,
                    course.CreatedByNavigation.Role
                },
                teacher = new
                {
                    teacher.FullName,
                    teacher.Email,
                    teacher.Role
                }
            });
        }




        // API to delete a course (only Admin can delete)
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{courseId}")]
        public IActionResult DeleteCourse([FromRoute] int courseId)
        {
            var courseToDelete = _courseService.GetCourseById(courseId);
            if (courseToDelete == null)
            {
                return NotFound(new { message = "Course does not exist." });
            }

            var isDeleted = _courseService.DeleteCourse(courseId);
            if (!isDeleted)
            {
                return BadRequest(new { message = "Course deletion failed." });
            }

            return Ok(new { message = "Course deleted successfully!" });
        }


        [Authorize]
        [HttpGet("my-courses")]
        public IActionResult GetUserCourses()
        {
            var currentUserIdClaim = User.FindFirst("IdUser");
            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "User information not found in token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            var courseList = _courseService.GetUserCourses(currentUserId);

            if (courseList == null || !courseList.Any())
            {
                return NotFound(new { message = "User has not enrolled in any course." });
            }

            return Ok(courseList);
        }



        [Authorize]
        [HttpPost("enroll/{courseId}")]
        public IActionResult EnrollInCourse([FromRoute] int courseId)
        {
            var currentUserIdClaim = User.FindFirst("IdUser");
            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "User information not found in token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);

            var isEnrolled = _courseService.EnrollInCourse(courseId, currentUserId);

            if (!isEnrolled)
            {
                return BadRequest(new { message = "Unable to enroll in course (course does not exist or you have already enrolled)." });
            }

            return Ok(new { message = "Course enrollment successful!" });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("confirm-enrollment/{courseId}/{studentId}")]
        public IActionResult ConfirmEnrollment(int courseId, int studentId)
        {
            var currentUserIdClaim = User.FindFirst("IdUser");
            if (currentUserIdClaim == null)
            {
                return Unauthorized(new { message = "User information not found in token." });
            }

            var currentUserId = int.Parse(currentUserIdClaim.Value);
            var currentUser = _userService.GetUserById(currentUserId);

            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Teacher"))
            {
                return Forbid(); // If the user is not Admin or Teacher
            }

            var result = _courseService.ConfirmEnrollment(courseId, studentId);

            if (!result)
            {
                return NotFound(new { message = "No enrollment found with Pending status." });
            }

            return Ok(new { message = "Student enrollment confirmed successfully." });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("students-courses")]
        public IActionResult GetStudentsAndCourses([FromQuery] string enrollmentStatus)
        {
            // Kiểm tra nếu không có giá trị trạng thái đăng ký, thì mặc định sử dụng "Confirmed"
            if (string.IsNullOrEmpty(enrollmentStatus))
            {
                enrollmentStatus = "Confirmed"; // Có thể thay đổi trạng thái mặc định nếu cần
            }

            var studentsAndCourses = _courseService.GetStudentsAndCourses(enrollmentStatus);

            if (studentsAndCourses == null || !studentsAndCourses.Any())
            {
                return NotFound(new { message = $"There are no students enrolled with status {enrollmentStatus}." });
            }

            return Ok(studentsAndCourses);
        }



        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("confirmed-students/{courseId}")]
        public IActionResult GetConfirmedStudentsInCourse(int courseId)
        {
            var studentsInCourse = _courseService.GetConfirmedStudentsInCourse(courseId);

            if (studentsInCourse == null || !studentsInCourse.Any())
            {
                return NotFound(new { message = "No confirmed students enrolled in this course." });
            }

            return Ok(studentsInCourse);
        }
        //iter 4
        [Authorize(Roles = "Admin, Teacher")]
        [HttpGet("unassigned-courses")]
        public IActionResult GetUnassignedCourses()
        {
            var unassignedCourses = _courseService.GetUnassignedCourses();
            if (unassignedCourses == null || !unassignedCourses.Any())
            {
                return NotFound(new { message = "No unassigned courses found." });
            }

            return Ok(unassignedCourses);
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpGet("students-not-enrolled")]
        public IActionResult GetStudentsNotEnrolled()
        {
            var students = _courseService.GetStudentsNotEnrolled();
            if (students == null || !students.Any())
            {
                return NotFound(new { message = "All students are enrolled in courses." });
            }

            return Ok(students);
        }




        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost("assign-teacher/{courseId}")]
        public IActionResult AssignTeacherToCourse(int courseId, [FromBody] int teacherId)
        {
            var course = _courseService.GetCourseById(courseId);
            if (course == null)
            {
                return NotFound(new { message = "Course not found." });
            }

            var teacherAssigned = _courseService.AssignTeacherToCourse(courseId, teacherId);
            if (!teacherAssigned)
            {
                return BadRequest(new { message = "Failed to assign teacher." });
            }

            return Ok(new { message = "Teacher assigned successfully!" });
        }

    }
}
