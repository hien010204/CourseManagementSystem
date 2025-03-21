﻿using CourseManagementSystem.Models;

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
        public User GetUserByUsernameOrEmail(string usernameOrEmail);
        public List<User> GetStudentsByFullName(string fullname, string role);
        bool CheckUsernameExists(string username);
        bool CheckEmailExists(string email);
        void UpdateUser(User user);
        void ChangePassword(User user);
        User AddUser(User user); // Thêm phương thức thêm người dùng
        void Logout();
        void SavePasswordResetCode(int idUser, string verificationCode);
        string GetPasswordResetCode(int idUser);
        public List<User> GetUserByStatusAndRole(string status, string role);
    }
}
