using CourseManagementSystem.DTO;
using CourseManagementSystem.Services.Announcements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAnnouncementsByCourse(int courseId)
        {
            var announcements = await _announcementService.GetAnnouncementsByCourseAsync(courseId);
            if (announcements == null || !announcements.Any())
            {
                return NotFound("No announcements found for the specified course.");
            }
            return Ok(announcements);
        }

        [HttpGet("get-announcement-byId/{announcementId}")]
        public async Task<ActionResult<AnnouncementDto>> GetAnnouncementById(int announcementId)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(announcementId);
            if (announcement == null)
            {
                return NotFound($"Announcement with ID {announcementId} not found.");
            }
            return Ok(announcement);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("create-announcement")]
        public async Task<ActionResult> CreateAnnouncement([FromBody] AnnouncementDto announcementDto)
        {
            if (announcementDto == null || string.IsNullOrEmpty(announcementDto.Title) || string.IsNullOrEmpty(announcementDto.Content))
            {
                return BadRequest("Invalid data. Title and Content are required.");
            }

            // Lấy UserID từ claim
            var userId = int.Parse(User.FindFirstValue("IdUser"));

            // Gán UserID vào announcementDto
            announcementDto.CreatedBy = userId;

            // Tạo thông báo
            await _announcementService.CreateAnnouncementAsync(announcementDto);

            // Trả về kết quả
            return CreatedAtAction(nameof(GetAnnouncementById), new { announcementId = announcementDto.AnnouncementID }, announcementDto);
        }


        [Authorize(Roles = "Admin,Teacher")]
        [HttpPut("update/{announcementId}")]
        public async Task<ActionResult> UpdateAnnouncement(int announcementId, [FromBody] AnnouncementDto announcementDto)
        {
            if (announcementDto == null || string.IsNullOrEmpty(announcementDto.Title) || string.IsNullOrEmpty(announcementDto.Content))
            {
                return BadRequest("Invalid data. Title and Content are required.");
            }

            // Lấy UserID từ claim
            var userId = int.Parse(User.FindFirstValue("IdUser"));

            // Gán UserID vào announcementDto
            announcementDto.CreatedBy = userId;

            try
            {
                await _announcementService.UpdateAnnouncementAsync(announcementId, announcementDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Announcement with ID {announcementId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{announcementId}")]
        public async Task<ActionResult> DeleteAnnouncement(int announcementId)
        {
            // Lấy UserID từ claim
            var userId = int.Parse(User.FindFirstValue("IdUser"));

            try
            {
                await _announcementService.DeleteAnnouncementAsync(announcementId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Announcement with ID {announcementId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
