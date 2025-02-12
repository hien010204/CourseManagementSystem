using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Models;

namespace CourseManagementSystem.Services.Courses
{
    public class CourseService : ICourseService
    {
        private readonly CourseManagementContext _context;

        public CourseService(CourseManagementContext context)
        {
            _context = context;
        }

        public Course AddCourse(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
            return course;
        }
    }
}