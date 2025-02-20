using CourseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementSystem.Services.QnA
{
    public class QnAService : IQnAService
    {
        private readonly CourseManagementContext _context;

        public QnAService(CourseManagementContext context)
        {
            _context = context;
        }

        // ✅ Submit a Question
        public async Task<Question> SubmitQuestionAsync(Question question)
        {
            if (question == null || string.IsNullOrWhiteSpace(question.Title) ||
                string.IsNullOrWhiteSpace(question.Content) || question.UserId <= 0)
            {
                throw new ArgumentException("Invalid question data. UserId is required and must be valid.");
            }

            // Kiểm tra UserID có tồn tại không
            var userExists = await _context.Users.AnyAsync(u => u.IdUser == question.UserId);
            if (!userExists)
            {
                throw new ArgumentException("UserId does not exist.");
            }

            question.CreatedAt = DateTime.UtcNow;
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }


        // ✅ Get All Questions (Optimized)
        public async Task<IEnumerable<Question>> GetQuestionsAsync()
        {
            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Select(q => new Question
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Content = q.Content,
                    CreatedAt = q.CreatedAt,
                    User = new User
                    {
                        IdUser = q.User.IdUser,
                        FullName = q.User.FullName,
                        Email = q.User.Email
                    },
                    Answers = q.Answers.Select(a => new Answer
                    {
                        AnswerId = a.AnswerId,
                        QuestionId = q.QuestionId, // ✅ Ensure mapping
                        Content = a.Content,
                        CreatedAt = a.CreatedAt,
                        User = new User
                        {
                            IdUser = a.User.IdUser,
                            FullName = a.User.FullName
                        }
                    }).ToList()
                }).ToListAsync();
        }

        // ✅ Get a Specific Question by ID
        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid question ID.");

            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Comments)
                        .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(q => q.QuestionId == id);
        }

        // ✅ Submit an Answer (Ensure question exists & valid UserId)
        public async Task<Answer> SubmitAnswerAsync(Answer answer)
        {
            if (answer == null || string.IsNullOrWhiteSpace(answer.Content) || answer.UserId <= 0)
            {
                throw new ArgumentException("Invalid answer data. UserId is required.");
            }

            // Kiểm tra UserID có tồn tại không
            var userExists = await _context.Users.AnyAsync(u => u.IdUser == answer.UserId);
            if (!userExists)
            {
                throw new ArgumentException("UserId does not exist.");
            }

            // Kiểm tra QuestionID có tồn tại không
            var questionExists = await _context.Questions.AnyAsync(q => q.QuestionId == answer.QuestionId);
            if (!questionExists)
            {
                throw new ArgumentException("QuestionId does not exist.");
            }

            answer.CreatedAt = DateTime.UtcNow;
            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }


        // ✅ Add a Comment (Ensure answer exists & valid UserId)
        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.Content) || comment.UserId <= 0)
                throw new ArgumentException("Invalid comment data.");

            var answerExists = await _context.Answers.AnyAsync(a => a.AnswerId == comment.AnswerId);
            if (!answerExists)
                throw new ArgumentException("Answer ID does not exist.");

            comment.CreatedAt = DateTime.UtcNow;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}
