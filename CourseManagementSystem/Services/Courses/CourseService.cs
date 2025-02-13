using CourseManagementSystem.DTO;
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
            // Lấy danh sách các Enrollment của người dùng từ bảng Course_Enrollments
            var enrollments = _context.CourseEnrollments
                                       .Where(e => e.StudentId == userId)
                                       .Select(e => e.CourseId)
                                       .ToList();

            // Dùng danh sách CourseID để truy vấn thông tin khóa học
            var courses = _context.Courses
                                  .Where(c => enrollments.Contains(c.CourseId))
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

        // Phương thức đăng ký khóa học
        public bool EnrollInCourse(int courseId, int userId)
        {
            // Kiểm tra xem khóa học có tồn tại không
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return false; // Khóa học không tồn tại
            }

            // Kiểm tra xem người dùng đã đăng ký khóa học này chưa
            var existingEnrollment = _context.CourseEnrollments
                                             .FirstOrDefault(e => e.CourseId == courseId && e.StudentId == userId);
            if (existingEnrollment != null)
            {
                return false; // Người dùng đã đăng ký khóa học này rồi
            }

            // Thêm người dùng vào khóa học
            var enrollment = new CourseEnrollment
            {
                CourseId = courseId,
                StudentId = userId,
                EnrollmentDate = DateTime.Now
            };

            _context.CourseEnrollments.Add(enrollment);
            _context.SaveChanges();

            return true; // Đăng ký thành công
        }

        public List<CourseDto> GetUserCourses(int userId)
        {
            // Lấy các khóa học mà người dùng đã đăng ký
            var userCourses = _context.Courses
                                      .Where(course => _context.CourseEnrollments
                                      .Any(enrollment => enrollment.CourseId == course.CourseId && enrollment.StudentId == userId))
                                      .ToList();

            if (userCourses == null || !userCourses.Any())
            {
                return null;  // Nếu không có khóa học nào
            }

            // Lấy thông tin khóa học và trạng thái đăng ký
            var courseList = userCourses.Select(course => new CourseDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                EnrollmentStatus = _context.CourseEnrollments
                                   .Where(e => e.CourseId == course.CourseId && e.StudentId == userId)
                                   .Select(e => e.EnrollmentStatus)  // Trạng thái đăng ký từ CourseEnrollments
                                   .FirstOrDefault()  // Lấy trạng thái đầu tiên
            }).ToList();

            return courseList;
        }

        public bool ConfirmEnrollment(int courseId, int studentId)
        {
            // Kiểm tra xem khóa học và học sinh có tồn tại trong bảng CourseEnrollments không
            var enrollment = _context.CourseEnrollments
                                     .FirstOrDefault(e => e.CourseId == courseId && e.StudentId == studentId && e.EnrollmentStatus == "Pending");

            if (enrollment == null)
            {
                return false; // Nếu không có đăng ký nào ở trạng thái Pending
            }

            // Cập nhật trạng thái đăng ký thành Confirmed
            enrollment.EnrollmentStatus = "Confirmed";
            _context.SaveChanges();

            return true;
        }
    }
}