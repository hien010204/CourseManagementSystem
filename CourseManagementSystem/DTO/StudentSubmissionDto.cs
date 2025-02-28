namespace CourseManagementSystem.DTO
{
    public class StudentSubmissionDto
    {
        public int SubmissionId { get; set; } // ID của bài nộp
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string SubmissionLink { get; set; }
        public DateTime SubmissionDate { get; set; }
        public decimal? Grade { get; set; }  // Điểm bài nộp (nếu có)
        public string Feedback { get; set; } // Phản hồi của giáo viên
    }

}
