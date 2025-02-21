using CourseManagementSystem.DTO;
using CourseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementSystem.Services.Announcements
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly CourseManagementContext _context;

        public AnnouncementService(CourseManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByCourseAsync(int courseId)
        {
            var announcements = await _context.Announcements
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            return announcements.Select(a => new AnnouncementDto
            {
                AnnouncementID = a.AnnouncementId,
                CourseID = a.CourseId,
                Title = a.Title,
                Content = a.Content,
                CreatedAt = (DateTime)a.CreatedAt,
                CreatedBy = a.CreatedBy
            });
        }

        public async Task<AnnouncementDto> GetAnnouncementByIdAsync(int announcementId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);

            if (announcement == null) return null;

            return new AnnouncementDto
            {
                AnnouncementID = announcement.AnnouncementId,
                CourseID = announcement.CourseId,
                Title = announcement.Title,
                Content = announcement.Content,
                CreatedAt = (DateTime)announcement.CreatedAt,
                CreatedBy = announcement.CreatedBy
            };
        }

        public async Task CreateAnnouncementAsync(AnnouncementDto announcementDto)
        {
            var announcement = new Announcement
            {
                CourseId = announcementDto.CourseID,
                Title = announcementDto.Title,
                Content = announcementDto.Content,
                CreatedAt = DateTime.Now,
                CreatedBy = announcementDto.CreatedBy
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnnouncementAsync(int announcementId, AnnouncementDto announcementDto)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);

            if (announcement == null) throw new Exception("Announcement not found.");

            announcement.Title = announcementDto.Title;
            announcement.Content = announcementDto.Content;
            announcement.CreatedAt = DateTime.Now;
            announcement.CreatedBy = announcementDto.CreatedBy;

            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAnnouncementAsync(int announcementId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);

            if (announcement == null) throw new Exception("Announcement not found.");

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
        }
    }

}
