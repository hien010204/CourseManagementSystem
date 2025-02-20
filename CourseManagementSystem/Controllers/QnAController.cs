using CourseManagementSystem.Models;
using CourseManagementSystem.Services.QnA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QnAController : ControllerBase
    {
        private readonly IQnAService _qnAService;

        public QnAController(IQnAService qnAService)
        {
            _qnAService = qnAService;
        }

        // ✅ Submit a Question
        [Authorize]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuestion([FromBody] Question question)
        {
            if (question == null || string.IsNullOrWhiteSpace(question.Title) || string.IsNullOrWhiteSpace(question.Content))
                return BadRequest("Question title and content are required.");

            try
            {
                var result = await _qnAService.SubmitQuestionAsync(question);
                return CreatedAtAction(nameof(GetQuestion), new { id = result.QuestionId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // ✅ View All Questions
        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            try
            {
                var questions = await _qnAService.GetQuestionsAsync();
                if (questions == null || !questions.Any()) return NotFound("No questions found.");
                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // ✅ View Specific Question
        [HttpGet("view/{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            if (id <= 0) return BadRequest("Invalid question ID.");

            try
            {
                var question = await _qnAService.GetQuestionByIdAsync(id);
                if (question == null) return NotFound("Question not found.");

                return Ok(question);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // ✅ Answer a Question
        [Authorize]
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] Answer answer)
        {
            if (answer == null || string.IsNullOrWhiteSpace(answer.Content))
                return BadRequest("Answer content is required.");

            try
            {
                var result = await _qnAService.SubmitAnswerAsync(answer);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // ✅ Comment on an Answer
        [Authorize]
        [HttpPost("comment")]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.Content))
                return BadRequest("Comment content is required.");

            try
            {
                var result = await _qnAService.AddCommentAsync(comment);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

}
