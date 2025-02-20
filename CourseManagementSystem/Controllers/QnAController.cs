using CourseManagementSystem.DTO.SubmitQnA;
using CourseManagementSystem.Models;
using CourseManagementSystem.Services.QnA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QnAController : ControllerBase
    {
        private readonly IQnAService _qnAService;

        public QnAController(IQnAService qnAService)
        {
            _qnAService = qnAService;
        }

        // ✅ Xem danh sách câu hỏi (View Q&A)
        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            var questions = await _qnAService.GetQuestionsAsync();
            if (questions == null || !questions.Any()) return NotFound("Không tìm thấy câu hỏi nào.");
            return Ok(questions);
        }

        // ✅ Xem chi tiết câu hỏi cụ thể
        [HttpGet("view/{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            if (id <= 0) return BadRequest("ID câu hỏi không hợp lệ.");

            var question = await _qnAService.GetQuestionByIdAsync(id);
            if (question == null) return NotFound("Không tìm thấy câu hỏi.");

            return Ok(question);
        }

        // ✅ Đăng câu hỏi mới (Chỉ học sinh đăng câu hỏi)
        [Authorize(Roles = "Student")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuestion([FromBody] SubmitQuestionDTO questionDto)
        {
            var currentUserIdClaim = User.FindFirst("IdUser")?.Value;
            if (currentUserIdClaim == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Lấy ID người dùng (học sinh) và gán vào câu hỏi
            var result = await _qnAService.SubmitQuestionAsync(questionDto, int.Parse(currentUserIdClaim));
            return CreatedAtAction(nameof(GetQuestion), new { id = result.QuestionId }, result);
        }

        // ✅ Trả lời câu hỏi (Chỉ Admin và Teacher có quyền trả lời)
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDTO answerDto)
        {
            var currentUserIdClaim = User.FindFirst("IdUser")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            if (string.IsNullOrWhiteSpace(answerDto.Content))
                return BadRequest("Nội dung câu trả lời là bắt buộc.");

            var result = await _qnAService.SubmitAnswerAsync(answerDto, currentUserId);
            return Ok(result);
        }

        // ✅ Bình luận vào câu trả lời (Mọi người đều có thể bình luận)
        [Authorize(Roles = "Student,Teacher,Admin")]
        [HttpPost("comment")]
        public async Task<IActionResult> AddComment([FromBody] SubmitCommentDTO commentDto)
        {
            var currentUserIdClaim = User.FindFirst("IdUser")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            if (string.IsNullOrWhiteSpace(commentDto.Content))
                return BadRequest("Nội dung bình luận là bắt buộc.");

            var result = await _qnAService.AddCommentAsync(commentDto, currentUserId);
            return Ok(result);
        }
    }
}
