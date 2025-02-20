namespace CourseManagementSystem.DTO.Question
{
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }  // Tên người tạo câu hỏi
        public DateTime CreatedAt { get; set; }
        public List<AnswerDto> Answers { get; set; }
    }
}
