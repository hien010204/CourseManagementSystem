using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Users;

namespace CourseManagementSystem.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly CourseManagementContext _context;
        private readonly IUserService _userService;

        public ProfileService(CourseManagementContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public bool ChangePassword(int iduser, string currentPassword, string newPassword)
        {
            var existingUser = _userService.GetUserById(iduser);
            // Kiểm tra mật khẩu cũ
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, existingUser.PasswordHash))
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
