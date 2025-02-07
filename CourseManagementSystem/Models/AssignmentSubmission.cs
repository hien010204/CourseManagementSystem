using System;
using System.Collections.Generic;

namespace CourseManagementSystem.Models;

public partial class AssignmentSubmission
{
    public int SubmissionId { get; set; }

    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
