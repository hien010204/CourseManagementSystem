namespace CourseManagementSystem.DTO.ScheduleDTO
{
    public class GetSchedulesByTeacherDto
    {
        public int TeacherID { get; set; }
        public List<ScheduleDto> Schedules { get; set; }
    }
}
