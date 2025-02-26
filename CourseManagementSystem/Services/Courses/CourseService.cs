using CourseManagementSystem.DTO;
using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Models;
using Microsoft.EntityFrameworkCore;

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
            return _context.Courses
                .Include(c => c.CreatedByNavigation)  // Bao gồm thông tin người tạo (User)
                .FirstOrDefault(c => c.CourseId == courseId);
        }


        public List<Course> GetCoursesByUserId(int userId)
        {
            var courses = (from e in _context.CourseEnrollments
                           join c in _context.Courses on e.CourseId equals c.CourseId
                           where e.StudentId == userId
                           select new Course
                           {
                               CourseId = c.CourseId,
                               CourseName = c.CourseName,
                               Description = c.Description,
                               StartDate = c.StartDate,
                               EndDate = c.EndDate
                           }).ToList();
            return courses;
        }

        public List<Course> GetAllCourses()
        {
            // Lấy danh sách tất cả khóa học từ cơ sở dữ liệu
            return _context.Courses.Select(c => new Course
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            }).ToList();

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
        // dành cho Student xem thử đã tham gia và đăng kí khoá học nào
        public List<CourseDto> GetUserCourses(int userId)
        {
            // Lấy các khóa học mà người dùng đã đăng ký
            var userCourses = _context.Courses
                                      .Where(course => _context.CourseEnrollments
                                      .Any(enrollment => enrollment.CourseId == course.CourseId && enrollment.StudentId == userId))
                                      .Include(course => course.CreatedByNavigation)
                                      .ToList();

            if (userCourses == null || !userCourses.Any())
            {
                return null;
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
                                   .FirstOrDefault(), // Lấy trạng thái đầu tiên
                CreatedByFullName = course.CreatedByNavigation.FullName,  // Lấy tên người tạo
                CreatedByRole = course.CreatedByNavigation.Role
            }).ToList();

            return courseList;
        }

        // Xác nhận đăng ký học viên vào khóa học --> dành cho teacher và admin
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

        // xem tất cả danh sách sinh viên tham gia khoá học bao gồm ( trạng thái đăng kí, được xác nhận và loại bỏ) --> dành cho teacher và admin
        public List<StudentCourseInfoDto> GetStudentsAndCourses(string enrollmentStatus)
        {
            var query = from enrollment in _context.CourseEnrollments
                        join user in _context.Users on enrollment.StudentId equals user.IdUser
                        join course in _context.Courses on enrollment.CourseId equals course.CourseId
                        where user.Role == "Student" && enrollment.EnrollmentStatus == enrollmentStatus  // Lọc theo trạng thái đăng ký truyền vào
                        select new StudentCourseInfoDto
                        {
                            StudentId = user.IdUser,
                            StudentName = user.FullName,
                            Email = user.Email,
                            CourseId = course.CourseId,
                            CourseName = course.CourseName,
                        };

            return query.ToList();
        }


        //lấy danh sách sinh viên đã tham gia khoá học này
        public List<StudentCourseInfoDto> GetConfirmedStudentsInCourse(int courseId)
        {
            var query = from enrollment in _context.CourseEnrollments
                        join user in _context.Users on enrollment.StudentId equals user.IdUser
                        join course in _context.Courses on enrollment.CourseId equals course.CourseId
                        where enrollment.EnrollmentStatus == "Confirmed"  // Lọc sinh viên đã xác nhận tham gia
                              && course.CourseId == courseId  // Lọc theo khóa học cụ thể
                        select new StudentCourseInfoDto
                        {
                            StudentId = user.IdUser,
                            StudentName = user.FullName,
                            Email = user.Email,
                            CourseId = course.CourseId,
                            CourseName = course.CourseName,

                        };

            return query.ToList();
        }

        public List<Course> GetCourseByName(string courseName)
        {
            if (string.IsNullOrEmpty(courseName))
            {
                return null;
            }
            var result = _context.Courses.FirstOrDefault(c => c.CourseName.ToLower().Contains(courseName.ToLower()));
            if (result == null)
            {
                return null;
            }
            return _context.Courses.Where(c => c.CourseName.ToLower().Contains(courseName.ToLower())).ToList();
        }
        //iter 4
        public List<CourseDto> GetUnassignedCourses()
        {
            // Lọc các khóa học mà không có giảng viên gán (dựa vào bảng Schedules)
            var unassignedCourses = _context.Courses
                .Where(c => !c.Schedules.Any(s => s.TeacherId != null))  // Kiểm tra các khóa học không có giảng viên
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate
                })
                .ToList();

            return unassignedCourses;
        }

        // Lấy sinh viên chưa đăng ký khóa học
        public List<UserDto> GetStudentsNotEnrolled()
        {
            var students = _context.Users
                .Where(u => u.Role == "Student" && !_context.CourseEnrollments
                    .Any(ce => ce.StudentId == u.IdUser))
                .Select(u => new UserDto
                {
                    UserId = u.IdUser,
                    FullName = u.FullName,
                    Email = u.Email
                })
                .ToList();

            return students;
        }


        // Gán giảng viên cho khóa học
        public bool AssignTeacherToCourse(int courseId, int teacherId)
        {
            // Tìm khóa học theo CourseId
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return false;  // Nếu không tìm thấy khóa học
            }

            // Kiểm tra xem giảng viên có tồn tại trong hệ thống và có vai trò "Teacher"
            var teacher = _context.Users.FirstOrDefault(u => u.IdUser == teacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return false;  // Nếu không tìm thấy giảng viên
            }

            _context.SaveChanges();  // Lưu thay đổi vào cơ sở dữ liệu
            return true;  // Gán giảng viên thành công
        }


    }
}