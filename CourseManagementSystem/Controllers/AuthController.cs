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


        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromForm] string usernameOrEmail)
        {
            // Kiểm tra nếu người dùng có email hoặc tên người dùng đã đăng ký không
            var user = _userService.GetUserByUsernameOrEmail(usernameOrEmail);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản với tên người dùng hoặc email này." });
            }

            // Tạo mã xác minh hoặc liên kết thay đổi mật khẩu
            var verificationCode = Guid.NewGuid().ToString(); // Tạo một mã xác minh ngẫu nhiên

            // Lưu mã xác minh vào cơ sở dữ liệu hoặc gửi qua email
            // Giả sử chúng ta gửi mã qua email
            try
            {
                // Tạo dịch vụ email và cấu hình email cần gửi
                var emailService = new EmailService(_config);
                var subject = "Yêu cầu thay đổi mật khẩu";
                var body = $"Xin chào {user.UserName},\n\nBạn đã yêu cầu thay đổi mật khẩu.\n\nSử dụng mã xác minh sau để tạo mật khẩu mới: {verificationCode}\n\nNếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.\n\nTrân trọng,\nĐội ngũ hỗ trợ";

                // Gửi email xác nhận
                emailService.SendEmail(user.Email, subject, body);

                // Lưu mã xác minh vào cơ sở dữ liệu để đối chiếu khi người dùng cung cấp mã này
                _userService.SavePasswordResetCode(user.IdUser, verificationCode);
            }
            catch (SmtpCommandException smtpEx)
            {
                return BadRequest(new { message = "Lỗi khi kết nối với dịch vụ gửi email. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Gửi email thất bại.", errorDetails = ex.Message });
            }

            return Ok(new { message = "Mã xác minh đã được gửi đến email của bạn. Vui lòng kiểm tra email để thay đổi mật khẩu." });
        }

        [HttpPut("reset-password")]
        public IActionResult ResetPassword([FromForm] string usernameOrEmail, [FromForm] string verificationCode, [FromForm] string newPassword)
        {
            // Kiểm tra người dùng và mã xác minh
            var user = _userService.GetUserByUsernameOrEmail(usernameOrEmail);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản với tên người dùng hoặc email này." });
            }

            // Kiểm tra mã xác minh
            var storedVerificationCode = _userService.GetPasswordResetCode(user.IdUser);
            if (storedVerificationCode != verificationCode)
            {
                return BadRequest(new { message = "Mã xác minh không hợp lệ." });
            }

            // Cập nhật mật khẩu cho người dùng
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            _userService.ChangePassword(user);

            return Ok(new { message = "Mật khẩu của bạn đã được thay đổi thành công." });
        }


    }
}