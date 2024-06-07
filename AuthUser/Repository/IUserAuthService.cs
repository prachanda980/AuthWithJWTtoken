using AuthUser.DTOs;
using AuthUser.Models;

namespace AuthUser.Repository
{
    public interface IUserAuthService
    {
        Task<User> RegisterUser(UserDTOs request);
        Task<AuthResponseDTOs> UserLogin(UserDTOs request);
        Task<AuthResponseDTOs> RefreshToken();

    }
}
