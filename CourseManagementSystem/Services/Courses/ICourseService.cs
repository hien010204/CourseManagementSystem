
using CourseManagementSystem.DTO;
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
        bool EnrollInCourse(int courseId, int userId);
        List<CourseDto> GetUserCourses(int userId);
        public bool ConfirmEnrollment(int courseId, int studentId);
        public List<StudentCourseInfoDto> GetStudentsAndCourses(string enrollmentStatus);
        public List<StudentCourseInfoDto> GetConfirmedStudentsInCourse(int courseId);
        public List<Course> GetCourseByName(string courseName);
    }
}
