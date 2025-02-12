using CourseManagementSystem.Models;
using CourseManagementSystem.Services.Email;
using CourseManagementSystem.Services.Users;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace CourseManagementSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
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
        public IActionResult Register([FromForm] string FullName, [FromForm] string username, [FromForm] string email, [FromForm] string password, [FromForm] string phonenumber)
        {
            // Kiểm tra xem username đã tồn tại chưa
            if (_userService.CheckUsernameExists(username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }

            //Kiểm tra xem email đã tồn tại chưa
            if (_userService.CheckEmailExists(email))
            {
                return BadRequest(new { message = "Email đã tồn tại." });
            }

            // Tạo user mới với mật khẩu đã mã hóa
            var newUser = new User
            {
                UserName = username,
                PasswordHash = password, // Mã hóa mật khẩu
                FullName = FullName,
                Email = email,
                PhoneNumber = phonenumber,
                CreatedAt = DateTime.UtcNow,
                Role = "Student"  // Loại người dùng mặc định
            };

            var createdUser = _userService.Register(newUser);

            if (createdUser == null)
            {
                return BadRequest(new { message = "Đăng ký không thành công." });
            }

            // Gửi email xác nhận đăng ký thành công
            try
            {
                // Tạo dịch vụ email và cấu hình email cần gửi
                var emailService = new EmailService(_config);
                var subject = "Chào mừng bạn đã đăng ký thành công tại Cousera";
                var body = $"Xin chào {newUser.UserName},\n\nCảm ơn bạn đã đăng ký tài khoản tại Cousera.\n\nChúng tôi hy vọng bạn sẽ có những trải nghiệm tuyệt vời khi sử dụng dịch vụ của chúng tôi.\n\nTrân trọng,\nĐội ngũ Cousera";

                // Gửi email xác nhận
                emailService.SendEmail(newUser.Email, subject, body);
            }
            catch (SmtpCommandException smtpEx)
            {
                // Nếu lỗi SMTP (ví dụ: không thể kết nối tới server hoặc xác thực thất bại)
                // Thông báo lỗi rõ ràng hơn
                return BadRequest(new { message = "Lỗi khi kết nối với dịch vụ gửi email. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung (chưa xác định rõ nguyên nhân)
                Console.WriteLine($"General Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);  // Cung cấp thông tin chi tiết về lỗi
                return BadRequest(new { message = "Gửi email xác nhận thất bại.", errorDetails = ex.Message });
            }



            return Ok(new
            {
                UserId = createdUser.IdUser,
                FullName = createdUser.FullName,
                Username = createdUser.UserName,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,

            });

        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _userService.Logout();
            return Ok(new { message = "Đã đăng xuất thành công" });
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

    }
}