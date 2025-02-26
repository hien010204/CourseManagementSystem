namespace CourseManagementSystem.DTO
{
    public class UserDto
    {
        public int UserId { get; set; }  // ID của người dùng
        public string FullName { get; set; }  // Tên đầy đủ của người dùng
        public string Email { get; set; }  // Email của người dùng
        public string Role { get; set; }  // Vai trò của người dùng (Admin, Teacher, Student, etc.)
    }
}
