using CourseManagementSystem.DTO;
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

        // Get all assignments for a specific course
        [HttpGet("get-assignments/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetAssignments(int courseId)
        {
            var assignments = await _assignmentService.GetAssignmentsByCourseId(courseId);
            return Ok(assignments);
        }

        //Get a specific assignment by its ID
        [HttpGet("get-assignment/{assignmentId}")]
        [Authorize]
        public async Task<IActionResult> GetAssignment(int assignmentId)
        {
            var assignment = await _assignmentService.GetAssignmentById(assignmentId);
            if (assignment == null)
                return NotFound("Assignment not found.");
            return Ok(assignment);
        }

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

        [HttpGet("get-submissions/{assignmentId}")]
        [Authorize(Roles = "Teacher")]
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

    }
}
