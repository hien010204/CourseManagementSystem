using CourseManagementSystem.DTO;
using CourseManagementSystem.DTO.UserDTO;
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
        public List<StudentsNotEnrolledDto> GetStudentsNotEnrolled()
        {
            var students = _context.Users
                .Where(u => u.Role == "Student" && !_context.CourseEnrollments
                    .Any(ce => ce.StudentId == u.IdUser))
                .Select(u => new StudentsNotEnrolledDto
                {
                    UserId = u.IdUser,
                    Role = u.Role,
                    FullName = u.FullName,
                    Status = u.Status,
                    Email = u.Email
                })
                .ToList();

            return students;
        }


        // Hàm gán giảng viên cho khóa học với kiểm tra xem đã có giáo viên chưa
        public (bool Success, string ErrorMessage) AssignTeacherToCourse(int courseId, int teacherId)
        {
            // Tìm khóa học theo CourseId
            var course = _context.Courses
                .Include(c => c.Schedules) // Bao gồm thông tin lịch học để kiểm tra
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null)
            {
                return (false, "Course not found."); // Khóa học không tồn tại
            }

            // Kiểm tra xem khóa học đã có giáo viên nào được gán chưa
            var existingTeacher = course.Schedules.FirstOrDefault(s => s.TeacherId != null);
            if (existingTeacher != null)
            {
                var currentTeacher = _context.Users
                    .FirstOrDefault(u => u.IdUser == existingTeacher.TeacherId);
                return (false, $"This course already has a teacher assigned: {currentTeacher?.FullName ?? "Unknown"}.");
            }

            // Kiểm tra xem giảng viên có tồn tại trong hệ thống và có vai trò "Teacher"
            var teacher = _context.Users
                .FirstOrDefault(u => u.IdUser == teacherId && u.Role == "Teacher");

            if (teacher == null)
            {
                return (false, "Teacher not found or user is not a teacher."); // Giảng viên không tồn tại hoặc không phải giáo viên
            }

            // Thêm giảng viên vào khóa học trong bảng Schedules
            var schedule = new Schedule
            {
                CourseId = courseId,
                TeacherId = teacherId
            };

            _context.Schedules.Add(schedule);
            _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            return (true, "Teacher assigned successfully."); // Gán giảng viên thành công
        }

        public User GetTeacherByCourseId(int courseId)
        {
            // Tìm khóa học theo CourseId và bao gồm lịch học (Schedules)
            var course = _context.Courses
                .Include(c => c.Schedules)  // Bao gồm lịch học (Schedules) cho khóa học
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null)
            {
                return null;  // Nếu không tìm thấy khóa học
            }

            // Lấy lịch học đầu tiên có TeacherId (giảng viên)
            var schedule = course.Schedules.FirstOrDefault(s => s.TeacherId != null);

            if (schedule == null)
            {
                return null;  // Nếu không có giảng viên nào được gán
            }

            // Lấy thông tin giảng viên từ bảng Users dựa trên TeacherId
            var teacher = _context.Users
                .FirstOrDefault(u => u.IdUser == schedule.TeacherId && u.Role == "Teacher");

            return teacher; // Trả về giảng viên
        }

        // Xoá giảng viên khỏi khóa học
        public (bool Success, string ErrorMessage) RemoveTeacherFromCourse(int courseId)
        {
            var course = _context.Courses
                .Include(c => c.Schedules)
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null)
            {
                return (false, "Course not found.");
            }

            var schedulesWithTeacher = course.Schedules.Where(s => s.TeacherId != null).ToList();
            if (!schedulesWithTeacher.Any())
            {
                return (false, "No teacher is assigned to this course.");
            }

            _context.Schedules.RemoveRange(schedulesWithTeacher); // Xóa tất cả lịch học có giảng viên
            _context.SaveChanges();

            return (true, "Teacher removed from course successfully.");
        }
        public List<CourseDto> GetCoursesByTeacherId(int teacherId)
        {
            var courses = _context.Courses
                .Where(c => c.Schedules.Any(s => s.TeacherId == teacherId)) // Lọc các khóa học mà giảng viên đang dạy
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CreatedByFullName = c.CreatedByNavigation.FullName,
                    CreatedByRole = c.CreatedByNavigation.Role
                })
                .ToList();

            return courses;
        }
        public List<CourseActiveDto> GetAllActiveCourses()
        {
            // Truy vấn dữ liệu thô từ cơ sở dữ liệu
            var activeCoursesQuery = _context.Courses
                .Include(c => c.CreatedByNavigation) // Tải thông tin người tạo (User)
                .Where(c =>
                    // Điều kiện 1: Có học sinh tham gia với trạng thái "Confirmed"
                    c.CourseEnrollments.Any(ce => ce.EnrollmentStatus == "Confirmed") &&
                    // Điều kiện 2: Có giáo viên dạy (Schedules có TeacherId)
                    c.Schedules.Any(s => s.TeacherId != null) &&
                    // Điều kiện 3: Có lịch học (Schedules không rỗng)
                    c.Schedules.Any() &&
                    // Điều kiện 4: Có lịch học mà Room không null
                    c.Schedules.Any(s => s.Room != null)
                )
                .Select(c => new
                {
                    Course = c,
                    // Lấy lịch đầu tiên có TeacherId và Room không null
                    Schedule = c.Schedules.FirstOrDefault(s => s.TeacherId != null && s.Room != null)
                })
                .Where(item => item.Schedule != null) // Loại bỏ các bản ghi không có Schedule phù hợp
                .ToList(); // Thực thi truy vấn và đưa dữ liệu vào bộ nhớ

            // Ánh xạ dữ liệu sang CourseActiveDto với xử lý null
            var activeCourses = activeCoursesQuery.Select(item => new CourseActiveDto
            {
                CourseId = item.Course.CourseId,
                TeacherId = item.Schedule.TeacherId, // Xử lý null cho TeacherId
                ScheduleId = item.Schedule.ScheduleId,
                CourseName = item.Course.CourseName ?? string.Empty, // Xử lý null cho CourseName
                Description = item.Course.Description ?? string.Empty, // Xử lý null cho Description
                StartDate = item.Course.StartDate,
                EndDate = item.Course.EndDate,
                ScheduleDate = item.Schedule.ScheduleDate,
                EnrollmentStatus = "Active", // Giá trị mặc định
                CreatedByFullName = item.Course.CreatedByNavigation?.FullName ?? string.Empty, // Đã tải, nhưng vẫn xử lý null cho an toàn
                CreatedByRole = item.Course.CreatedByNavigation?.Role ?? string.Empty, // Đã tải, nhưng vẫn xử lý null cho an toàn
                Room = item.Schedule.Room // Room đã được lọc không null
            }).ToList();

            return activeCourses;
        }

    }
}