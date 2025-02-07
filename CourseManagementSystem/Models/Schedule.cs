using System;
using System.Collections.Generic;

namespace CourseManagementSystem.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public DateOnly ScheduleDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
