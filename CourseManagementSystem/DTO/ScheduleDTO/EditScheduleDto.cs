namespace CourseManagementSystem.DTO.ScheduleDTO
{
    public class EditScheduleDto
    {
        public int TeacherID { get; set; }
        public DateOnly ScheduleDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Room { get; set; }
    }

}
