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

        public Course EditCourse(Course course)
        {
            // Find the existing course by its ID
            var existingCourse = _context.Courses.FirstOrDefault(c => c.CourseId == course.CourseId);

            if (existingCourse == null)
            {
                // Course not found, you can return null or throw an exception
                return null;  // Or throw new Exception("Course not found");
            }

            // Update the properties of the existing course
            existingCourse.CourseName = course.CourseName;
            existingCourse.Description = course.Description;
            existingCourse.StartDate = course.StartDate;
            existingCourse.EndDate = course.EndDate;

            // You can update other properties here as needed

            // Save the changes to the database
            _context.SaveChanges();

            // Return the updated course
            return existingCourse;
        }
        public Course GetCourseById(int courseId)
        {
            return _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
        }

        public List<Course> GetCoursesByUserId(int userId)
        {
            var courses = _context.Courses
                .Join(_context.CourseEnrollments,
                      course => course.CourseId,
                      enrollment => enrollment.CourseId,
                      (course, enrollment) => new { course, enrollment })
                .Where(x => x.enrollment.StudentId == userId)
                .Select(x => x.course)
                .ToList();

            return courses;
        }
        public List<Course> GetAllCourses()
        {
            // Lấy danh sách tất cả khóa học từ cơ sở dữ liệu
            return _context.Courses.ToList();
        }

        public bool DeleteCourse(int courseId)
        {
            // Kiểm tra khóa học có tồn tại không
            var courseToDelete = _context.Courses.Find(courseId);
            if (courseToDelete == null)
            {
                return false;  // Nếu khóa học không tồn tại, trả về false
            }

            // Xóa các thông báo liên quan đến khóa học
            var announcements = _context.Announcements.Where(a => a.CourseId == courseId);
            _context.Announcements.RemoveRange(announcements);

            // Xóa lịch học liên quan đến khóa học
            var schedules = _context.Schedules.Where(s => s.CourseId == courseId);
            _context.Schedules.RemoveRange(schedules);

            // Lấy tất cả các bài tập liên quan đến khóa học
            var assignments = _context.Assignments
                .Where(a => a.CourseId == courseId)
                .ToList();  // Lấy danh sách các bài tập của khóa học

            // Lấy tất cả các bài nộp của học viên liên quan đến các bài tập của khóa học
            var assignmentSubmissions = _context.AssignmentSubmissions
                .Where(e => assignments.Select(a => a.AssignmentId).Contains(e.AssignmentId))
                .ToList();  // Lấy tất cả các bài nộp của học viên
            _context.AssignmentSubmissions.RemoveRange(assignmentSubmissions);

            // Xóa các bài tập liên quan đến khóa học
            _context.Assignments.RemoveRange(assignments);

            // Xóa học viên tham gia khóa học
            var courseEnrollments = _context.CourseEnrollments.Where(ce => ce.CourseId == courseId);
            _context.CourseEnrollments.RemoveRange(courseEnrollments);

            // Cuối cùng xóa khóa học chính
            _context.Courses.Remove(courseToDelete);

            // Lưu tất cả các thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            return true;
        }




    }
}