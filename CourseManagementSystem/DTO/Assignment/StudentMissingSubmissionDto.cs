namespace CourseManagementSystem.DTO.Assignment
{
    public class StudentMissingSubmissionDto
    {
        public int StudentId { get; set; } // Sử dụng IdUser từ User
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
    }
}
