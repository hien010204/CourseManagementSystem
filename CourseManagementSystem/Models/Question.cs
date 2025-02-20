namespace CourseManagementSystem.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int UserId { get; set; } // 🔥 UserId phải bắt buộc có giá trị

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 🔥 Đảm bảo không null

    public virtual ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();

    public virtual User? User { get; set; } // 🔥 Dùng nullable tránh lỗi null reference
}
