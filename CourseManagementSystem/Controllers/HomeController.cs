using CourseManagementSystem.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public HomeController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("all-courses")]
        public IActionResult GetAllCourses()
        {
            var courses = _courseService.GetAllCourses();
            if (courses == null || !courses.Any())
            {
                return NotFound(new { message = "No courses available." });
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

        [HttpGet("search/{coursename}")]
        public IActionResult SearchCourse([FromRoute] string coursename)
        {
            var course = _courseService.GetCourseByName(coursename);
            if (course == null)
            {
                return NotFound(new { message = "Course not found." });
            }
            var courseList = course.Select(course => new
            {
                course.CourseId,
                course.CourseName,
                course.Description,
                course.StartDate,
                course.EndDate,
            }).ToList();
            return Ok(courseList);
        }

    }
}
