using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly CourseManagementContext _context;

        public ProfileService(CourseManagementContext context)
        {
            _context = context;
        }

        public bool ChangePassword(User user, string currentPassword, string newPassword)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.IdUser == user.IdUser);
            if (existingUser == null)
            {
                return false;
            }

            // Kiểm tra mật khẩu cũ
            if (existingUser.PasswordHash != BCrypt.Net.BCrypt.HashPassword(currentPassword)) // Giả sử mật khẩu chưa mã hóa
            {
                return false;
            }

            // Cập nhật mật khẩu mới
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            existingUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(existingUser);
            _context.SaveChanges();

            return true;
        }
        // Phương thức cập nhật thông tin người dùng
        public bool UpdateProfile(User user, string fullName, string email, string phoneNumber)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.IdUser == user.IdUser);
            if (existingUser == null)
            {
                return false;
            }

            // Cập nhật thông tin người dùng
            existingUser.FullName = fullName;
            existingUser.Email = email;
            existingUser.PhoneNumber = phoneNumber;
            existingUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(existingUser);
            _context.SaveChanges();

            return true;
        }

    }
}
