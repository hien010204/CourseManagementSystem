namespace CourseManagementSystem.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int AnswerId { get; set; } // 🔥 Đảm bảo bắt buộc có giá trị

    public int UserId { get; set; } // 🔥 Đảm bảo bắt buộc có giá trị

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 🔥 Đảm bảo không null

    public virtual Answer? Answer { get; set; } // 🔥 Dùng nullable (?) để tránh lỗi khi dữ liệu chưa load

    public virtual User? User { get; set; } // 🔥 Dùng nullable (?) để tránh lỗi nếu User chưa được load
}
