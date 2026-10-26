using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        List<UserDTO> Get();

        UserDTO? Get(int id);

        bool Create(UserDTO c);

        bool Update(UserDTO c);

        List<UserDTO> Search(string? term);

        bool Delete(int id);

        UserDTO? GetByEmail(string? email);

        Dictionary<int, string> GetAvatars(IEnumerable<int> ids);

        bool SetAvatar(int userId, string? avatarPath);

        Dictionary<int, string> GetNames(IEnumerable<int> ids);
    }
}
