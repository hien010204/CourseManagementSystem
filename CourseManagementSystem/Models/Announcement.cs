using System;
using System.Collections.Generic;

namespace CourseManagementSystem.Models;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public int CourseId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;
}
