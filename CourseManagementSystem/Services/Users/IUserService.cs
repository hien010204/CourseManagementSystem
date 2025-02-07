using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Users
{
    public interface IUserService
    {
        User Authenticate(string username, string password);

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        /// <param name="user">Thông tin người dùng</param>
        /// <returns>Trả về thông tin người dùng sau khi đăng ký, nếu không thành công trả về null</returns>
        User Register(User user);
        List<User> GetAllUsers();
        User GetUserById(int userId);

        bool CheckUsernameExists(string username);
        bool CheckEmailExists(string email);
        void UpdateUser(User user);
        void ChangePassword(User user);
        void DeleteUser(int userId); // Thêm phương thức xóa người dùng
        User AddUser(User user); // Thêm phương thức thêm người dùng
        void Logout();
    }
}
