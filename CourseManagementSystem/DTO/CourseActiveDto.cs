namespace CourseManagementSystem.DTO
{
    public class CourseActiveDto
    {
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public int ScheduleId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly? ScheduleDate { get; set; }
        public string EnrollmentStatus { get; set; }
        public string CreatedByFullName { get; set; }
        public string CreatedByRole { get; set; }
        public string? Room { get; set; }
    }
}
