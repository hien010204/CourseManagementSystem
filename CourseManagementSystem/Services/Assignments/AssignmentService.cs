using CourseManagementSystem.DTO;
using CourseManagementSystem.DTO.Assignment;
using CourseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementSystem.Services.Assignments
{
    public class AssignmentService : IAssignmentService
    {
        private readonly CourseManagementContext _context;
        public AssignmentService(CourseManagementContext context)
        {
            _context = context;
        }

        public async Task<List<Assignment>> GetAssignmentsByCourseId(int courseId)
        {
            // Truy vấn bảng Assignments để lấy tất cả bài tập của khóa học với courseId
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)  // Lọc theo CourseID
                .Include(o => o.AssignmentSubmissions)  // Kết nối với bảng AssignmentSubmissions để lấy thông tin bài nộp
                .ToListAsync();  // Chuyển đổi kết quả thành danh sách

            return assignments;
        }

        public async Task<Assignment> GetAssignmentById(int assignmentId)
        {
            // Truy vấn bảng Assignments để lấy bài tập theo AssignmentID
            var assignment = await _context.Assignments
                .Include(a => a.AssignmentSubmissions)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);// Lọc theo AssignmentID


            return assignment;  // Trả về bài tập tìm được
        }

        public async Task<Assignment> CreateAssignment(CreateAssignmentDto assignmentDto)
        {
            // Tạo đối tượng bài tập mới từ DTO
            var assignment = new Assignment
            {
                CourseId = assignmentDto.CourseId,
                Title = assignmentDto.Title,
                Description = assignmentDto.Description,
                DueDate = assignmentDto.DueDate,
                CreatedAt = DateTime.Now  // Đặt thời gian tạo bài tập
            };

            // Thêm bài tập vào cơ sở dữ liệu
            _context.Assignments.Add(assignment);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về bài tập mới đã được tạo
            return assignment;
        }

        public async Task<Assignment> EditAssignment(int assignmentId, EditAssignmentDto assignmentDto)
        {
            // Tìm bài tập theo AssignmentId
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);  // Lọc theo AssignmentId

            // Kiểm tra nếu bài tập không tồn tại
            if (assignment == null)
            {
                return null;  // Trả về null nếu không tìm thấy bài tập
            }

            // Cập nhật các trường trong bài tập
            assignment.Title = assignmentDto.Title;
            assignment.Description = assignmentDto.Description;
            assignment.DueDate = assignmentDto.DueDate;  // Chuyển đổi DateTime sang DateOnly nếu cần

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về bài tập đã được chỉnh sửa
            return assignment;
        }

        public async Task<bool> DeleteAssignment(int assignmentId)
        {
            // Tìm bài tập theo AssignmentId
            var assignment = await _context.Assignments
                .Include(a => a.AssignmentSubmissions)  // Lấy các bài nộp liên quan
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);  // Lọc theo AssignmentId

            // Kiểm tra nếu bài tập không tồn tại
            if (assignment == null)
            {
                return false;  // Nếu không tìm thấy bài tập, trả về false
            }

            // Xóa các bài nộp liên quan trong bảng AssignmentSubmissions
            _context.AssignmentSubmissions.RemoveRange(assignment.AssignmentSubmissions);

            // Xóa bài tập
            _context.Assignments.Remove(assignment);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return true;  // Trả về true nếu bài tập và dữ liệu liên quan đã được xóa thành công
        }

        public async Task<AssignmentSubmissionDto> SubmitAssignment(int assignmentId, SubmitAssignmentDto submissionDto)
        {
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                return null; // Nếu bài tập không tồn tại
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                StudentId = submissionDto.StudentId,
                SubmissionLink = submissionDto.SubmissionLink,
                SubmissionDate = DateTime.Now
            };

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Trả về DTO thay vì đối tượng đầy đủ
            var submissionDtoResponse = new AssignmentSubmissionDto
            {
                AssignmentId = submission.AssignmentId,
                StudentId = submission.StudentId,
                SubmissionLink = submission.SubmissionLink,
                SubmissionDate = (DateTime)submission.SubmissionDate
            };

            return submissionDtoResponse;
        }

        public async Task<List<StudentSubmissionDto>> GetSubmissionsByAssignmentId(int assignmentId)
        {
            // Truy vấn bảng AssignmentSubmissions để lấy danh sách các bài nộp của sinh viên
            var submissions = await _context.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId) // Lọc theo AssignmentId
                .Include(s => s.Student)  // Kết nối với bảng Users để lấy thông tin sinh viên
                .Select(s => new StudentSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentId = s.StudentId,
                    StudentName = s.Student.FullName,  // Lấy tên sinh viên từ bảng Users
                    StudentEmail = s.Student.Email,   // Lấy email sinh viên từ bảng Users
                    SubmissionLink = s.SubmissionLink, // Link bài nộp
                    SubmissionDate = (DateTime)s.SubmissionDate,
                    Grade = s.Grade,                  // Điểm của bài nộp
                    Feedback = s.Feedback             // Phản hồi của giáo viên
                })
                .ToListAsync();

            return submissions; // Trả về danh sách bài nộp
        }

        //dùng để chấm điểm
        public async Task<AssignmentSubmission> GradeAssignment(int submissionId, GradeAssignmentDto gradeDto)
        {
            // Tìm bài nộp của học sinh theo ID
            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            // Kiểm tra xem bài nộp có tồn tại không
            if (submission == null)
            {
                return null;  // Nếu không tìm thấy bài nộp, trả về null
            }

            // Cập nhật điểm và phản hồi của giáo viên
            submission.Grade = gradeDto.Grade;
            submission.Feedback = gradeDto.Feedback;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.AssignmentSubmissions.Update(submission);
            await _context.SaveChangesAsync();

            return submission;  // Trả về bài nộp đã được chấm điểm và phản hồi
        }

        // list bài chưa chấm điểm
        public async Task<List<StudentSubmissionDto>> GetUngradedSubmissions()
        {
            var ungradedSubmissions = await _context.AssignmentSubmissions
                .Where(s => s.Grade == null)  // Lọc bài nộp chưa có điểm
                .Include(s => s.Student)  // Lấy thông tin sinh viên
                .OrderByDescending(s => s.SubmissionDate)  // Sắp xếp theo ngày nộp
                .Select(s => new StudentSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentName = s.Student.FullName,
                    SubmissionLink = s.SubmissionLink,
                    Grade = s.Grade,
                    Feedback = s.Feedback,
                    SubmissionDate = (DateTime)s.SubmissionDate
                })
                .ToListAsync();

            return ungradedSubmissions;
        }
        // list bài chưa có feedback
        public async Task<List<StudentSubmissionDto>> GetNoFeedbackSubmissions()
        {
            var noFeedbackSubmissions = await _context.AssignmentSubmissions
                .Where(s => s.Feedback == null)  // Lọc bài nộp chưa có phản hồi
                .Include(s => s.Student)  // Lấy thông tin sinh viên
                .OrderByDescending(s => s.SubmissionDate)  // Sắp xếp theo ngày nộp
                .Select(s => new StudentSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentName = s.Student.FullName,
                    SubmissionLink = s.SubmissionLink,
                    Grade = s.Grade,
                    Feedback = s.Feedback,
                    SubmissionDate = (DateTime)s.SubmissionDate
                })
                .ToListAsync();

            return noFeedbackSubmissions;
        }


        public async Task<StudentSubmissionDto> GetGradeAndFeedback(int submissionId)
        {
            // Tìm bài nộp với ID cụ thể
            var submission = await _context.AssignmentSubmissions
                .Where(s => s.SubmissionId == submissionId)
                .Select(s => new StudentSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentId = s.StudentId,          // Lấy ID của sinh viên
                    StudentName = s.Student.FullName, // Lấy tên của sinh viên
                    SubmissionLink = s.SubmissionLink, // Lấy link bài nộp
                    Grade = s.Grade,              // Lấy điểm của bài nộp
                    Feedback = s.Feedback         // Lấy phản hồi của bài nộp
                })
                .FirstOrDefaultAsync();

            // Nếu không tìm thấy bài nộp, trả về null
            if (submission == null)
            {
                return null;
            }

            // Trả về thông tin điểm và phản hồi của bài nộp
            return submission;
        }
        //list bài đã được chấm điểm và feedback
        public async Task<List<StudentSubmissionDto>> GetGradedAndFeedbackSubmissions()
        {
            var gradedSubmissions = await _context.AssignmentSubmissions
                .Where(s => s.Grade != null && s.Feedback != null)  // Lọc bài nộp có điểm và phản hồi
                .Include(s => s.Student)  // Lấy thông tin sinh viên
                .OrderByDescending(s => s.SubmissionDate)  // Sắp xếp theo ngày nộp
                .Select(s => new StudentSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentName = s.Student.FullName,
                    SubmissionLink = s.SubmissionLink,
                    Grade = s.Grade,
                    Feedback = s.Feedback,
                    SubmissionDate = (DateTime)s.SubmissionDate
                })
                .ToListAsync();

            return gradedSubmissions;
        }

        public async Task<List<Assignment>> FilterAssignmentsByDueDate(int courseId, DateOnly? dueDate)
        {
            var assignmentsQuery = _context.Assignments.Where(a => a.CourseId == courseId);
            if (dueDate.HasValue)
            {
                assignmentsQuery = assignmentsQuery.Where(a => a.DueDate <= dueDate.Value);
            }
            return await assignmentsQuery.ToListAsync();
        }

        public async Task<List<StudentMissingSubmissionDto>> GetStudentsMissingSubmission(int assignmentId)
        {
            // 1. Lấy thông tin bài tập và khóa học tương ứng
            var assignment = await _context.Assignments
                .Where(a => a.AssignmentId == assignmentId)
                .Include(a => a.Course)
                .ThenInclude(c => c.CourseEnrollments)
                .ThenInclude(ce => ce.Student)
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                throw new Exception("Assignment not found.");
            }

            // 2. Lấy danh sách sinh viên trong khóa học
            var studentsInCourse = assignment.Course.CourseEnrollments
                .Select(ce => new StudentMissingSubmissionDto
                {
                    StudentId = ce.Student.IdUser,
                    StudentName = ce.Student.FullName,
                    StudentEmail = ce.Student.Email
                })
                .ToList();

            // 3. Lấy danh sách sinh viên đã nộp bài
            var submittedStudents = await GetSubmissionsByAssignmentId(assignmentId);
            var submittedStudentIds = submittedStudents.Select(s => s.StudentId).ToList();

            // 4. Lọc danh sách sinh viên chưa nộp bài
            var studentsMissingSubmission = studentsInCourse
                .Where(s => !submittedStudentIds.Contains(s.StudentId))
                .ToList();

            return studentsMissingSubmission;
        }

        public async Task<AssignmentSubmission> EditGradeAndFeedback(int submissionId, GradeAssignmentDto gradeDto)
        {
            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                return null;

            submission.Grade = gradeDto.Grade;
            submission.Feedback = gradeDto.Feedback;

            _context.AssignmentSubmissions.Update(submission);
            await _context.SaveChangesAsync();

            return submission;
        }

        public async Task<AssignmentSubmission> EditSubmission(int submissionId, int studentId, EditSubmissionDto editSubmissionDto)
        {
            // Tìm bài nộp theo submissionId
            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId && s.StudentId == studentId);

            // Kiểm tra nếu bài nộp không tồn tại hoặc không thuộc về học sinh hiện tại
            if (submission == null)
            {
                return null; // Trả về null nếu không tìm thấy hoặc không có quyền
            }

            // Cập nhật thông tin bài nộp
            submission.SubmissionLink = editSubmissionDto.SubmissionLink ?? submission.SubmissionLink;
            submission.SubmissionDate = DateTime.Now; // Cập nhật ngày nộp mới

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.AssignmentSubmissions.Update(submission);
            await _context.SaveChangesAsync();

            return submission; // Trả về bài nộp đã được cập nhật
        }
    }
}