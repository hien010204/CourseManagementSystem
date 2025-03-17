using CourseManagementSystem.DTO;
using CourseManagementSystem.DTO.UserDTO;
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
        bool DeleteCourse(int courseId);
        bool EnrollInCourse(int courseId, int userId);
        List<CourseDto> GetUserCourses(int userId);
        bool ConfirmEnrollment(int courseId, int studentId);
        List<StudentCourseInfoDto> GetStudentsAndCourses(string enrollmentStatus);
        List<StudentCourseInfoDto> GetConfirmedStudentsInCourse(int courseId);
        List<Course> GetCourseByName(string courseName);
        List<CourseDto> GetUnassignedCourses();
        List<StudentsNotEnrolledDto> GetStudentsNotEnrolled();
        (bool Success, string ErrorMessage) AssignTeacherToCourse(int courseId, int teacherId); // Cập nhật kiểu trả về
        User GetTeacherByCourseId(int courseId);
        (bool Success, string ErrorMessage) RemoveTeacherFromCourse(int courseId);//xoá giảng viên khỏi khoá học 
        List<CourseDto> GetCoursesByTeacherId(int teacherId);// list cac khoa hoc ma giang vien day
        List<CourseActiveDto> GetAllActiveCourses();
    }
}