namespace CourseManagementSystem.DTO.ScheduleDTO
{
    public class EditScheduleDto
    {
        public DateOnly ScheduleDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Room { get; set; }
    }

}
