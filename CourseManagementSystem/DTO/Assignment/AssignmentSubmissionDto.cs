namespace CourseManagementSystem.DTO.Assignment
{
    public class AssignmentSubmissionDto
    {

        public int AssignmentId { get; set; } // ID của bài tập
        public int StudentId { get; set; }    // ID của sinh viên
        public string Feedback { get; set; }  // Phản hồi của sinh viên
        public string SubmissionLink { get; set; } // Link bài nộp
        public DateTime SubmissionDate { get; set; } // Ngày nộp bài
    }

}
