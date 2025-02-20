using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.QnA
{
    public interface IQnAService
    {
        Task<Question> SubmitQuestionAsync(Question question);
        Task<IEnumerable<Question>> GetQuestionsAsync();
        Task<Question> GetQuestionByIdAsync(int id);
        Task<Answer> SubmitAnswerAsync(Answer answer);
        Task<Comment> AddCommentAsync(Comment comment);
    }
}
