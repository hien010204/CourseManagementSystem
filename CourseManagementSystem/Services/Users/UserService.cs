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
            if (user.Role == "Admin")
            {
                if (user.PasswordHash == password)
                {
                    return user; // Đăng nhập thành công cho admin với mật khẩu không mã hóa
                }
                return null;
            }

            // Nếu không phải admin, kiểm tra bcrypt cho mật khẩu đã mã hóa
            if (user.Role != "Admin" && !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Mật khẩu không khớp
            }

            return user;  // Trả về user nếu thông tin hợp lệ
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
                    FullName = u.FullName,
                    Email = u.Email,
                    Status = u.Status,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber,
                    PasswordHash = u.PasswordHash,
                    CreatedAt = u.CreatedAt
                }).FirstOrDefault();
        }
        public User GetUserByUsernameOrEmail(string usernameOrEmail)
        {
            return _context.Users
                .FirstOrDefault(u => u.FullName == usernameOrEmail || u.Email == usernameOrEmail);
        }

        public List<User> GetStudentsByFullName(string fullname, string role)
        {
            return _context.Users
        .Where(u => u.FullName.ToLower().Contains(fullname.ToLower()) && u.Role == role) // Case-insensitive search
        .Select(u => new User
        {
            IdUser = u.IdUser,
            UserName = u.UserName,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            Status = u.Status,
            CreatedAt = u.CreatedAt
        })
        .ToList();

        }
        public void Logout()
        {

        }

        public User Register(User user)
        {

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Thêm user mới vào database
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.IdUser == user.IdUser);
            if (existingUser != null)
            {
                existingUser.UpdatedAt = user.UpdatedAt;  // Cập nhật trạng thái
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
                existingUser.UpdatedAt = user.UpdatedAt;  // Cập nhật trạng thái
                existingUser.PasswordHash = user.PasswordHash;
                _context.Users.Update(existingUser);
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
            if (user.Role == "Admin")
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return user;  // Trả về thông tin người dùng sau khi đăng ký thành công
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Mã hóa mật khẩu trước khi lưu


            // Thêm user mới vào database
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;  // Trả về thông tin người dùng sau khi đăng ký thành công
        }

        public void SavePasswordResetCode(int idUser, string verificationCode)
        {
            // Tìm người dùng theo Id
            var user = _context.Users.FirstOrDefault(u => u.IdUser == idUser);

            // Nếu người dùng không tồn tại, ném lỗi
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Lưu mã xác minh và thời gian hết hạn (1 giờ từ hiện tại)
            user.PasswordResetCode = verificationCode;
            user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddHours(1);  // Mã sẽ hết hạn sau 1 giờ

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();
        }

        public string GetPasswordResetCode(int idUser)
        {
            // Tìm người dùng theo Id
            var user = _context.Users.FirstOrDefault(u => u.IdUser == idUser);

            // Nếu người dùng không tồn tại, trả về null hoặc ném lỗi
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Kiểm tra nếu mã đã hết hạn
            if (user.PasswordResetCodeExpiresAt < DateTime.UtcNow)
            {
                return null;  // Mã đã hết hạn
            }

            // Trả về mã xác minh còn hiệu lực
            return user.PasswordResetCode;
        }

        public bool CheckEmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public List<User> GetUserByStatusAndRole(string status, string role)
        {
            return _context.Users
            .Where(u => u.Status == status && u.Role == role) // Filter by both Status and Role
            .Select(u => new User
            {
                IdUser = u.IdUser,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            })
            .ToList();
        }

    }
}
