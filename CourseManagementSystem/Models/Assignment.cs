﻿using System;
using System.Collections.Generic;

namespace CourseManagementSystem.Models;

public partial class Assignment
{
    public int AssignmentId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual Course Course { get; set; } = null!;
}
