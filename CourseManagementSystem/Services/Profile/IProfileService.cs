using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Profile
{
    public interface IProfileService
    {
        bool ChangePassword(int iduser, string currentPassword, string newPassword);
        bool UpdateProfile(User user, string fullName, string email, string phoneNumber);

    }
}
