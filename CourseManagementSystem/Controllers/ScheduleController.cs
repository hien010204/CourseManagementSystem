using CourseManagementSystem.DTO.ScheduleDTO;
using CourseManagementSystem.Services.Schedules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost("add-schedule")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> AddSchedule([FromBody] AddScheduleDto addScheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var schedule = await _scheduleService.AddSchedule(addScheduleDto);
            return CreatedAtAction(nameof(GetSchedule), new { courseId = schedule.CourseId }, schedule);
        }

        [HttpPut("edit-schedule/{scheduleId}")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> EditSchedule(int scheduleId, [FromBody] EditScheduleDto editScheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedSchedule = await _scheduleService.EditSchedule(scheduleId, editScheduleDto);
            if (updatedSchedule == null)
                return NotFound("Schedule not found.");

            return Ok(updatedSchedule);
        }

        [HttpGet("get-schedule/{courseId}")]
        public async Task<IActionResult> GetSchedule(int courseId)
        {
            var schedules = await _scheduleService.GetSchedulesByCourseId(courseId);
            if (schedules == null || !schedules.Any())
                return NotFound("No schedules found for this course.");

            return Ok(schedules);
        }

        [HttpGet("filter-schedule-by-date/{courseId}")]
        [Authorize(Roles = "Teacher, Student")]
        public async Task<IActionResult> FilterScheduleByDate(int courseId, [FromQuery] DateOnly scheduleDate)
        {
            var schedules = await _scheduleService.GetSchedulesByDate(courseId, scheduleDate);
            if (schedules == null || !schedules.Any())
                return NotFound("No schedules found for this date.");

            return Ok(schedules);
        }



    }
}
