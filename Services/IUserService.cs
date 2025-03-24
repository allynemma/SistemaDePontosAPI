using SistemaDePontosAPI.Model;

namespace SistemaDePontosAPI.Services
{
    public interface IUserService
    {
        Task<Users> Register(Users user);
        Users Authenticate(string email, string password);
        Task<Users> GetUserById(int id);
        Task<IEnumerable<Users>> GetAllUsers();
        Task<Users> UpdateUser(int id, Users updateUser);
        Task<bool> DeleteUser(int id);
        string GenerateJwtToken(string email, int userId);
    }
}
