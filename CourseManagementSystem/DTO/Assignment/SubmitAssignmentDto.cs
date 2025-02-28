namespace CourseManagementSystem.DTO.Assignment
{
    public class SubmitAssignmentDto
    {
        public int StudentId { get; set; }  // ID của học viên
        public string? SubmissionLink { get; set; }  // Link bài nộp
    }

}
