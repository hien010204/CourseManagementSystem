namespace CourseManagementSystem.DTO.Question
{
    public class AnswerDto
    {
        public int AnswerId { get; set; }
        public string Content { get; set; }
        public string AnsweredBy { get; set; }  // Tên người trả lời
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
