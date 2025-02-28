namespace CourseManagementSystem.DTO.Assignment
{
    public class EditAssignmentDto
    {
        public string Title { get; set; }  // Tiêu đề mới của bài tập
        public string Description { get; set; }  // Mô tả mới của bài tập

        public DateOnly DueDate { get; set; }  // Ngày hết hạn mới của bài tập
    }

}
