using CourseManagementSystem.DTO.Question;
using CourseManagementSystem.DTO.SubmitQnA;
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

        //  Submit a Question (User can submit, Admin/Teacher can reply, everyone can view)
        public async Task<Question> SubmitQuestionAsync(SubmitQuestionDTO questionDto, int userId)
        {
            if (questionDto == null || string.IsNullOrWhiteSpace(questionDto.Title) ||
                string.IsNullOrWhiteSpace(questionDto.Content) || userId <= 0)
            {
                throw new ArgumentException("Invalid question data. UserId is required and must be valid.");
            }

            var question = new Question
            {
                UserId = userId,
                Title = questionDto.Title,
                Content = questionDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        // Get All Questions
        public async Task<IEnumerable<QuestionDto>> GetQuestionsAsync()
        {
            return await _context.Questions
                .Include(q => q.User)  // Thêm người tạo câu hỏi
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)  // Thêm người trả lời
                    .ThenInclude(a => a.Comments)  // Thêm bình luận cho câu trả lời
                .Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Content = q.Content,
                    CreatedBy = q.User.FullName,  // Chỉ lấy tên người tạo câu hỏi
                    CreatedAt = (DateTime)q.CreatedAt,
                    Answers = q.Answers.Select(a => new AnswerDto
                    {
                        AnswerId = a.AnswerId,
                        Content = a.Content,
                        AnsweredBy = a.User.FullName,  // Chỉ lấy tên người trả lời
                        CreatedAt = (DateTime)a.CreatedAt,
                        Comments = a.Comments.Select(c => new CommentDto
                        {
                            CommentId = c.CommentId,
                            Content = c.Content,
                            CommentedBy = c.User.FullName,  // Chỉ lấy tên người bình luận
                            CreatedAt = (DateTime)c.CreatedAt
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();
        }



        // Get a Specific Question by ID
        public async Task<QuestionDto> GetQuestionByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid question ID.");

            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                    .ThenInclude(a => a.Comments)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
                throw new KeyNotFoundException("Question not found.");

            return new QuestionDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Content = question.Content,
                CreatedBy = question.User.FullName,  // Tên người tạo câu hỏi
                CreatedAt = (DateTime)question.CreatedAt,
                Answers = question.Answers?.Select(a => new AnswerDto
                {
                    AnswerId = a.AnswerId,
                    Content = a.Content,
                    AnsweredBy = a.User.FullName,  // Tên người trả lời
                    CreatedAt = (DateTime)a.CreatedAt,
                    Comments = a.Comments?.Select(c => new CommentDto
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        CommentedBy = c.User.FullName,  // Tên người bình luận
                        CreatedAt = (DateTime)c.CreatedAt
                    }).ToList() ?? new List<CommentDto>()  // Nếu Comments là null, trả về danh sách trống
                }).ToList() ?? new List<AnswerDto>()  // Nếu Answers là null, trả về danh sách trống
            };
        }




        // ✅ Submit an Answer (Admin/Teacher can reply)
        public async Task<Answer> SubmitAnswerAsync(SubmitAnswerDTO answerDto, int userId)
        {
            if (answerDto == null || string.IsNullOrWhiteSpace(answerDto.Content) || userId <= 0)
            {
                throw new ArgumentException("Invalid answer data. UserId is required.");
            }

            var answer = new Answer
            {
                UserId = userId,
                QuestionId = answerDto.QuestionId,
                Content = answerDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        // ✅ Add a Comment (Users can comment, everyone can see)
        public async Task<Comment> AddCommentAsync(SubmitCommentDTO commentDto, int userId)
        {
            if (commentDto == null || string.IsNullOrWhiteSpace(commentDto.Content) || userId <= 0)
                throw new ArgumentException("Invalid comment data.");

            var comment = new Comment
            {
                UserId = userId,
                AnswerId = commentDto.AnswerId,
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}
