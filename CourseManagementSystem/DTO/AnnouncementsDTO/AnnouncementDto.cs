using System.Text.Json.Serialization;

namespace CourseManagementSystem.DTO.AnnouncementsDTO
{
    public class AnnouncementDto
    {
        [JsonIgnore]
        public int AnnouncementID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }

}
