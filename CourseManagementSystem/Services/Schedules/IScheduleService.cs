using CourseManagementSystem.DTO.ScheduleDTO;

namespace CourseManagementSystem.Services.Schedules
{
    public interface IScheduleService
    {
        // Lấy lịch học cho một khóa học
        Task<List<ScheduleDto>> GetSchedulesByCourseId(int courseId);

        // Thêm lịch học mới
        Task<ScheduleDto> AddSchedule(AddScheduleDto addScheduleDto);

        // Chỉnh sửa lịch học
        Task<ScheduleDto> EditSchedule(int scheduleId, EditScheduleDto editScheduleDto);

        // Xóa lịch học
        Task<bool> DeleteSchedule(int scheduleId);
        // lọc lịch học theo ngày
        public Task<List<ScheduleDto>> GetSchedulesByDate(int courseId, DateOnly scheduleDate);
    }
}
