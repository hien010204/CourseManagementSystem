using CourseManagementSystem.DTO.ScheduleDTO;
using CourseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementSystem.Services.Schedules
{
    public class ScheduleService : IScheduleService
    {
        private readonly CourseManagementContext _context;

        public ScheduleService(CourseManagementContext context)
        {
            _context = context;
        }

        // Lấy lịch học cho 1 khóa học
        public async Task<List<ScheduleDto>> GetSchedulesByCourseId(int courseId)
        {
            var schedules = await _context.Schedules
                .Where(s => s.CourseId == courseId)
                .Include(s => s.Teacher) // Include teacher info
                .Select(s => new ScheduleDto
                {
                    CourseId = s.CourseId,
                    ScheduleDate = s.ScheduleDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Room = s.Room,
                    TeacherName = s.Teacher.FullName
                })
                .ToListAsync();

            return schedules;
        }

        // Thêm lịch học mới
        public async Task<ScheduleDto> AddSchedule(AddScheduleDto addScheduleDto)
        {
            var schedule = new Schedule
            {
                CourseId = addScheduleDto.CourseID,
                TeacherId = addScheduleDto.TeacherID,
                ScheduleDate = addScheduleDto.ScheduleDate,
                StartTime = addScheduleDto.StartTime,
                EndTime = addScheduleDto.EndTime,
                Room = addScheduleDto.Room
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return new ScheduleDto
            {
                CourseId = schedule.CourseId,
                ScheduleDate = schedule.ScheduleDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Room = schedule.Room,
                TeacherName = schedule.Teacher.FullName
            };
        }

        // Chỉnh sửa lịch học
        public async Task<ScheduleDto> EditSchedule(int scheduleId, EditScheduleDto editScheduleDto)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null)
                return null; // Nếu không tìm thấy lịch học

            schedule.ScheduleDate = editScheduleDto.ScheduleDate;
            schedule.StartTime = editScheduleDto.StartTime;
            schedule.EndTime = editScheduleDto.EndTime;
            schedule.Room = editScheduleDto.Room;
            schedule.TeacherId = editScheduleDto.TeacherID;

            await _context.SaveChangesAsync();

            return new ScheduleDto
            {
                CourseId = schedule.CourseId,
                ScheduleDate = schedule.ScheduleDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Room = schedule.Room,
                TeacherName = schedule.Teacher.FullName
            };
        }

        // Xóa lịch học
        public async Task<bool> DeleteSchedule(int scheduleId)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null)
                return false; // Không tìm thấy lịch học

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return true; // Xóa thành công
        }

        public async Task<List<ScheduleDto>> GetSchedulesByDate(int courseId, DateOnly scheduleDate)
        {
            var schedules = await _context.Schedules
                .Where(s => s.CourseId == courseId && s.ScheduleDate == scheduleDate) // Lọc theo CourseId và ScheduleDate
                .Include(s => s.Teacher) // Bao gồm thông tin giảng viên
                .Select(s => new ScheduleDto
                {
                    CourseId = s.CourseId,
                    ScheduleDate = s.ScheduleDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Room = s.Room,
                    TeacherName = s.Teacher.FullName
                })
                .ToListAsync();

            return schedules;
        }

    }
}
