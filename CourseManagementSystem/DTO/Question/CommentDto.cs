namespace CourseManagementSystem.DTO.Question
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string CommentedBy { get; set; }  // Tên người bình luận
        public DateTime CreatedAt { get; set; }
    }
}
