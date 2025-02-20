using System;
using System.Collections.Generic;

namespace CourseManagementSystem.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual User User { get; set; } = null!;
}
