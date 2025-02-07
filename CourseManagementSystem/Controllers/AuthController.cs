using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CourseManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public AuthController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string username, [FromForm] string password)
        {
            // Kiểm tra thông tin đăng nhập
            var user = _userService.Authenticate(username, password);

            if (user == null)
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });

            if (user.Status == "Inactive")
            {
                return StatusCode(403, new { message = "Tài khoản của bạn đã bị ban." }); // Trả về lỗi 403 với thông báo
            }

            // Tạo JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            // Định nghĩa các claims cho token


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("IdUser", user.IdUser.ToString()), // Lưu UserId từ bảng User
                    new Claim(ClaimTypes.Name, user.UserName),   // Lưu Username từ bảng User
                    new Claim(ClaimTypes.Role, user.Role)        // Lưu vai trò (role) của người dùng
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token có hiệu lực trong 1 giờ
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Trả về thông tin đăng nhập kèm token và type (role)
            return Ok(new
            {
                IdUser = user.IdUser,
                UserName = user.UserName,
                Role = user.Role,  // Thêm type vào thông tin trả về
                Status = user.Status,
                Token = tokenString,
                Expiration = tokenDescriptor.Expires
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromForm] string FullName, [FromForm] string username, [FromForm] string password, [FromForm] string email, [FromForm] string phonenumber, [FromForm] string Status)
        {
            // Kiểm tra xem username đã tồn tại chưa
            if (_userService.CheckUsernameExists(username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }

            // Kiểm tra xem email đã tồn tại chưa
            if (_userService.CheckEmailExists(email))
            {
                return BadRequest(new { message = "Email đã tồn tại." });
            }

            // Tạo user mới với mật khẩu đã mã hóa
            var newUser = new User
            {
                UserName = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // Mã hóa mật khẩu
                FullName = FullName,
                Email = email,
                PhoneNumber = phonenumber,
                CreatedAt = DateTime.UtcNow,
                Status = Status,
                Role = "Student"  // Loại người dùng mặc định
            };

            var createdUser = _userService.Register(newUser);

            if (createdUser == null)
            {
                return BadRequest(new { message = "Đăng ký không thành công." });
            }

            // Gửi email xác nhận đăng ký thành công
            /*
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("MyShop", _config["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress(newUser.UserName, newUser.Email));
            emailMessage.Subject = "Chào mừng bạn đã đăng ký thành công tại MyShop";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Xin chào {newUser.UserName},\n\nCảm ơn bạn đã đăng ký tài khoản tại MyShop.\n\nChúng tôi hy vọng bạn sẽ có những trải nghiệm tuyệt vời khi sử dụng dịch vụ của chúng tôi.\n\nTrân trọng,\nĐội ngũ MyShop"
            };
            */
            /*
            using (var client = new SmtpClient())
            {
                // Sử dụng StartTls cho cổng 587 hoặc SslOnConnect cho cổng 465
                object value = client.Connect(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"]);
                client.Send(emailMessage);
                client.Disconnect(true);
            }
            */
            // Trả về thông tin người dùng đã được tạo
            return Ok(new
            {
                UserId = createdUser.IdUser,
                FullName = createdUser.FullName,
                Username = createdUser.UserName,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,
                Status = createdUser.Status

            });

        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _userService.Logout();
            return Ok(new { message = "Đã đăng xuất thành công" });
        }


        [HttpPut("update-status/{userId}")]
        public IActionResult UpdateUserStatus(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Cập nhật trạng thái của người dùng
            user.Status = user.Status == "Active" ? "Inactive" : "Active";  // Đổi từ Active thành Inactive và ngược lại
            user.UpdatedAt = DateTime.UtcNow;
            _userService.UpdateUser(user);

            return Ok(new { message = "Trạng thái của người dùng đã được cập nhật.", newStatus = user.Status });

        }

        [HttpPut("change-password/{userId}")]
        public IActionResult ChangePassword(int userId, [FromForm] string passwordlast, [FromForm] string password)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }
            if (passwordlast == password && passwordlast != user.PasswordHash)
            {
                return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu cũ hoặc sai password" });
            }
            // Cập nhật trạng thái của người dùng
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.UpdatedAt = DateTime.UtcNow;
            _userService.ChangePassword(user);

            return Ok(new { message = "Password người dùng đã được cập nhật." });

        }
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers()
                .Select(user => new
                {
                    user.IdUser,
                    user.UserName,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    user.Status
                }).ToList();

            return Ok(users);
        }
    }
}