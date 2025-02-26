namespace CourseManagementSystem.DTO.ScheduleDTO
{
    public class ScheduleDto
    {
        public int CourseId { get; set; }
        public DateOnly ScheduleDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Room { get; set; }
        public string TeacherName { get; set; }
    }

}
