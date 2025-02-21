﻿using CourseManagementSystem.DTO;
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
                .ToListAsync();  // Chuyển đổi kết quả thành danh sách

            return assignments;
        }

        public async Task<Assignment> GetAssignmentById(int assignmentId)
        {
            // Truy vấn bảng Assignments để lấy bài tập theo AssignmentID
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);  // Lọc theo AssignmentID

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
                Feedback = submissionDto.Feedback,
                SubmissionDate = DateTime.Now
            };

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Trả về DTO thay vì đối tượng đầy đủ
            var submissionDtoResponse = new AssignmentSubmissionDto
            {
                AssignmentId = submission.AssignmentId,
                StudentId = submission.StudentId,
                Feedback = submission.Feedback,
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
                    SubmissionDate = (DateTime)s.SubmissionDate,
                    Grade = s.Grade,                  // Điểm của bài nộp
                    Feedback = s.Feedback             // Phản hồi của giáo viên
                })
                .ToListAsync();

            return submissions; // Trả về danh sách bài nộp
        }


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


    }
}