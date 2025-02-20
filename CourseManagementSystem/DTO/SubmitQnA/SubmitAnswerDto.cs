namespace CourseManagementSystem.DTO.SubmitQnA
{
    public class SubmitAnswerDTO
    {
        public string Content { get; set; }
        public int QuestionId { get; set; } // ID của câu hỏi mà người dùng muốn trả lời
    }
}
