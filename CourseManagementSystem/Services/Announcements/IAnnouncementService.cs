using CourseManagementSystem.DTO.AnnouncementsDTO;

namespace CourseManagementSystem.Services.Announcements
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByCourseAsync(int courseId);
        Task<AnnouncementDto> GetAnnouncementByIdAsync(int announcementId);
        Task CreateAnnouncementAsync(AnnouncementDto announcementDto);
        Task UpdateAnnouncementAsync(int announcementId, AnnouncementDto announcementDto);
        Task DeleteAnnouncementAsync(int announcementId);
        public Task<IEnumerable<AnnouncementDto>> SearchAnnouncementsByTitleAsync(string title);
    }

}
