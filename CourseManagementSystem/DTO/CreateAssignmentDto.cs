namespace CourseManagementSystem.DTO
{
    public class CreateAssignmentDto
    {
        public int CourseId { get; set; }  // ID của khóa học
        public string Title { get; set; }  // Tiêu đề bài tập
        public string Description { get; set; }  // Mô tả bài tập
        public DateOnly DueDate { get; set; }  // Ngày hết hạn bài tập
    }

}
