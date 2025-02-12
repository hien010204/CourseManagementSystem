
using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Models
{
    public interface ICourseService
    {
        Course AddCourse(Course course);
        Course EditCourse(Course course);
        Course GetCourseById(int courseId);
        List<Course> GetAllCourses();
        List<Course> GetCoursesByUserId(int userId);
        public bool DeleteCourse(int courseId);
    }
}
