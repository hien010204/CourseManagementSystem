using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Profile
{
    public interface IProfileService
    {
        bool ChangePassword(User user, string currentPassword, string newPassword);
        bool UpdateProfile(User user, string fullName, string email, string phoneNumber);

    }
}
