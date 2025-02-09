using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Users
{
    public class UserService : IUserService
    {
        private readonly CourseManagementContext _context;
        public UserService(CourseManagementContext context)
        {
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == username);

            // Nếu không tìm thấy user, trả về null
            if (user == null)
            {
                return null;
            }

            // Nếu user là admin, so sánh mật khẩu không mã hóa
            if (user.Role == "Admin" && user.PasswordHash == password)
            {
                return user; // Đăng nhập thành công cho admin với mật khẩu không mã hóa
            }

            // Nếu không phải admin, kiểm tra bcrypt cho mật khẩu đã mã hóa
            if (user.Role != "Admin" && !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Mật khẩu không khớp
            }

            return user;  // Trả về user nếu thông tin hợp lệ
        }

        public bool CheckEmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public bool CheckUsernameExists(string username)
        {
            return _context.Users.Any(u => u.UserName == username);
        }

        public List<User> GetAllUsers()
        {
            return _context.Users
                .Select(u => new User
                {
                    IdUser = u.IdUser,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt
                    // Không bao gồm Password
                }).ToList();

        }

        public User GetUserById(int userId)
        {
            return _context.Users
                .Where(u => u.IdUser == userId)
                .Select(u => new User
                {
                    IdUser = u.IdUser,
                    UserName = u.UserName,
                    Email = u.Email,
                    Status = u.Status,
                    Role = u.Role,
                    PasswordHash = u.PasswordHash,
                    CreatedAt = u.CreatedAt
                }).FirstOrDefault();
        }

        public void Logout()
        {

        }

        public User Register(User user)
        {
            // Kiểm tra xem username đã tồn tại chưa
            if (CheckUsernameExists(user.UserName))
            {
                return null;
            }

            // Kiểm tra xem email đã tồn tại chưa
            if (CheckEmailExists(user.Email))
            {
                return null;
            }

            // Mã hóa mật khẩu trước khi lưu
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Thêm user mới vào database
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;  // Trả về thông tin người dùng sau khi đăng ký thành công
        }

        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.IdUser == user.IdUser);
            if (existingUser != null)
            {
                existingUser.UpdatedAt = user.UpdatedAt;  // Cập nhật trạng thái hoặc các trường khác
                existingUser.Status = user.Status;
                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }
        }

        public void ChangePassword(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.IdUser == user.IdUser);
            if (existingUser != null)
            {
                existingUser.UpdatedAt = user.UpdatedAt;  // Cập nhật trạng thái hoặc các trường khác
                existingUser.PasswordHash = user.PasswordHash;
                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }
        }

        public void DeleteUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.IdUser == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // Thêm phương thức thêm người dùng
        public User AddUser(User user)
        {
            if (CheckUsernameExists(user.UserName))
            {
                return null;
            }

            // Kiểm tra xem email đã tồn tại chưa
            if (CheckEmailExists(user.Email))
            {
                return null;
            }

            // Mã hóa mật khẩu trước khi lưu
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Thêm user mới vào database
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;  // Trả về thông tin người dùng sau khi đăng ký thành công
        }
    }
}
