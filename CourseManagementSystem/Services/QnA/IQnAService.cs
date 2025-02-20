using CourseManagementSystem.DTO.Question;
using CourseManagementSystem.DTO.SubmitQnA;
using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.QnA
{
    public interface IQnAService
    {
        // Submit a Question (User can submit, Admin/Teacher can reply, everyone can view)
        Task<Question> SubmitQuestionAsync(SubmitQuestionDTO questionDto, int userId);

        // Get All Questions
        // Phương thức trả về danh sách câu hỏi dưới dạng DTO
        Task<IEnumerable<QuestionDto>> GetQuestionsAsync();

        // Phương thức trả về câu hỏi theo ID dưới dạng DTO
        Task<QuestionDto> GetQuestionByIdAsync(int id);


        // Submit an Answer (Admin/Teacher can reply)
        Task<Answer> SubmitAnswerAsync(SubmitAnswerDTO answerDto, int userId);

        // Add a Comment (Users can comment, everyone can see)
        Task<Comment> AddCommentAsync(SubmitCommentDTO commentDto, int userId);
    }
}
