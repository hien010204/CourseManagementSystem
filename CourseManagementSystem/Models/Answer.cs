using CourseManagementSystem.Models;
using System.Text.Json.Serialization;

public class Answer
{
    public int AnswerId { get; set; }
    public int QuestionId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]  // Không serialize thuộc tính này
    public virtual ICollection<Comment> Comments { get; set; }

    [JsonIgnore]  // Tránh vòng lặp giữa Answer và Question
    public virtual Question Question { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}
