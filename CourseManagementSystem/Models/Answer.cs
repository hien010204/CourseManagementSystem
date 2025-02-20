using System;
using System.Collections.Generic;

namespace CourseManagementSystem.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
