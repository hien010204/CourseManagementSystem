namespace CourseManagementSystem.DTO.SubmitQnA
{
    public class SubmitCommentDTO
    {
        public string Content { get; set; }
        public int AnswerId { get; set; } // ID của câu trả lời mà người dùng muốn bình luận
    }
}
