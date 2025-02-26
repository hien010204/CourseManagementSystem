using CourseManagementSystem.DTO.Assignment;
using CourseManagementSystem.Services.Assignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        // xem tất cả bài tập của khoá học
        [HttpGet("get-assignments/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetAssignments(int courseId)
        {
            var assignments = await _assignmentService.GetAssignmentsByCourseId(courseId);
            return Ok(assignments);
        }

        // xem bài tập
        [HttpGet("get-assignment/{assignmentId}")]
        [Authorize]
        public async Task<IActionResult> GetAssignment(int assignmentId)
        {
            var assignment = await _assignmentService.GetAssignmentById(assignmentId);
            if (assignment == null)
                return NotFound("Assignment not found.");
            return Ok(assignment);
        }

        // Tạo bài tập
        [HttpPost("create-assignment")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto assignmentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Kiểm tra tính hợp lệ của dữ liệu đầu vào

            var createdAssignment = await _assignmentService.CreateAssignment(assignmentDto);

            // Trả về bài tập mới với mã trạng thái HTTP 201 (Created)
            return CreatedAtAction(nameof(GetAssignment), new { assignmentId = createdAssignment.AssignmentId }, createdAssignment);
        }

        // Chỉnh sửa bài tập
        [HttpPut("edit-assignment/{assignmentId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> EditAssignment(int assignmentId, [FromBody] EditAssignmentDto assignmentDto)
        {
            // Kiểm tra tính hợp lệ của dữ liệu
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Nếu dữ liệu không hợp lệ, trả về lỗi

            // Gọi dịch vụ để chỉnh sửa bài tập
            var updatedAssignment = await _assignmentService.EditAssignment(assignmentId, assignmentDto);

            // Kiểm tra nếu bài tập không tồn tại
            if (updatedAssignment == null)
                return NotFound("Assignment not found.");

            // Trả về bài tập đã được chỉnh sửa
            return Ok(updatedAssignment);
        }

        // Xóa bài tập
        [HttpDelete("delete-assignment/{assignmentId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            // Gọi service để xóa bài tập
            var isDeleted = await _assignmentService.DeleteAssignment(assignmentId);

            // Kiểm tra nếu bài tập không tồn tại
            if (!isDeleted)
                return NotFound("Assignment not found.");

            // Trả về kết quả xóa thành công
            return NoContent();  // Trả về HTTP 204 No Content nếu xóa thành công
        }

        // nộp bài tập 
        [HttpPost("submit-assignment/{assignmentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, [FromBody] SubmitAssignmentDto submissionDto)
        {
            // Kiểm tra tính hợp lệ của dữ liệu
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Nếu dữ liệu không hợp lệ, trả về lỗi

            // Gọi dịch vụ để nộp bài tập
            var submission = await _assignmentService.SubmitAssignment(assignmentId, submissionDto);

            // Kiểm tra nếu bài tập không tồn tại
            if (submission == null)
                return NotFound("Assignment not found.");

            // Trả về bài nộp thành công
            return CreatedAtAction(nameof(GetAssignment), new { assignmentId = submission.AssignmentId }, submission);
        }
        // lấy danh sách bài nộp của sinh viên cho 1 bài tập cụ thể
        [HttpGet("get-submissions/{assignmentId}")]
        [Authorize]
        public async Task<IActionResult> GetSubmissions(int assignmentId)
        {
            // Gọi dịch vụ để lấy danh sách sinh viên nộp bài
            var submissions = await _assignmentService.GetSubmissionsByAssignmentId(assignmentId);

            // Kiểm tra nếu không có bài nộp nào
            if (submissions == null || !submissions.Any())
            {
                return NotFound("No submissions found for this assignment.");
            }

            // Trả về danh sách sinh viên đã nộp bài
            return Ok(submissions);
        }

        // chấm điểm bài nộp của học sinh
        [HttpPut("grade-assignment/{submissionId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GradeAssignment(int submissionId, [FromBody] GradeAssignmentDto gradeDto)
        {
            // Kiểm tra tính hợp lệ của dữ liệu
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Nếu dữ liệu không hợp lệ, trả về lỗi

            // Gọi dịch vụ để chấm điểm bài nộp
            var gradedSubmission = await _assignmentService.GradeAssignment(submissionId, gradeDto);

            // Kiểm tra nếu bài nộp không tồn tại
            if (gradedSubmission == null)
                return NotFound("Assignment submission not found.");

            // Trả về bài nộp đã được chấm điểm và phản hồi
            return Ok(gradedSubmission);
        }

        //iter4
        // List bài chưa chấm điểm
        [Authorize(Roles = "Teacher")]
        [HttpGet("ungraded-submissions")]
        public async Task<IActionResult> GetUngradedSubmissions()
        {
            var ungradedSubmissions = await _assignmentService.GetUngradedSubmissions();
            return Ok(ungradedSubmissions);
        }

        // List bài chưa có feedback
        [Authorize(Roles = "Teacher")]
        [HttpGet("no-feedback-submissions")]
        public async Task<IActionResult> GetNoFeedbackSubmissions()
        {
            var noFeedbackSubmissions = await _assignmentService.GetNoFeedbackSubmissions();
            return Ok(noFeedbackSubmissions);
        }

        // Lấy thông tin điểm và feedback của một bài nộp cụ thể
        [HttpGet("get-grade-feedback/{submissionId}")]
        public async Task<IActionResult> GetGradeAndFeedback(int submissionId)
        {
            var submission = await _assignmentService.GetGradeAndFeedback(submissionId);

            if (submission == null)
            {
                return NotFound("Assignment submission not found.");
            }

            return Ok(submission);
        }

        // List bài đã được chấm điểm và feedback
        [HttpGet("graded-feedback-submissions")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetGradedAndFeedbackSubmissions()
        {
            var gradedSubmissions = await _assignmentService.GetGradedAndFeedbackSubmissions();
            return Ok(gradedSubmissions);
        }

        // Lọc bài tập theo hạn nộp
        [HttpGet("filter-assignments/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> FilterAssignmentsByDueDate(int courseId, [FromQuery] DateOnly? dueDate)
        {
            var assignments = await _assignmentService.FilterAssignmentsByDueDate(courseId, dueDate);
            return Ok(assignments);
        }

        // Danh sách sinh viên chưa nộp bài
        [HttpGet("students-missing-submissions/{assignmentId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetStudentsMissingSubmission(int assignmentId)
        {
            var studentsMissingSubmission = await _assignmentService.GetStudentsMissingSubmission(assignmentId);
            return Ok(studentsMissingSubmission);
        }


        // Chỉnh sửa điểm và phản hồi
        [HttpPut("edit-grade-feedback/{submissionId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditGradeAndFeedback(int submissionId, [FromBody] GradeAssignmentDto gradeDto)
        {
            var updatedSubmission = await _assignmentService.EditGradeAndFeedback(submissionId, gradeDto);
            if (updatedSubmission == null)
                return NotFound("Assignment submission not found.");

            return Ok(updatedSubmission);
        }


    }
}
