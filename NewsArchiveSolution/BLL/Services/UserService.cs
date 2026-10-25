using AutoMapper;
using BLL.DTOs;
using BLL.Security;
using DAL.EF.Tables;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class UserService
    {
        private readonly IUserRepository repo;
        private readonly IMapper mapper;

        public UserService(DataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetUserRepository();
            this.mapper = mapper;
        }

        public List<UserDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<UserDTO>>(data);
            return res;
        }

        public UserDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<UserDTO>(data);
            return res;
        }

        public bool Create(UserDTO c)
        {
            var converted = mapper.Map<User>(c);
            converted.Name = converted.Name.Trim();
            converted.Email = converted.Email.Trim();
            converted.PasswordHash = PasswordHasher.Hash(c.PasswordHash);
            converted.CreatedAt = DateTime.UtcNow;
            return repo.Create(converted);
        }

        public bool Update(UserDTO c)
        {
            var existing = repo.Get(c.UserId);
            if (existing == null) return false;
            existing.Name = c.Name.Trim();
            existing.RoleId = c.RoleId;
            return repo.Update(existing);
        }

        public List<UserDTO> Search(string? term)
        {
            var users = Get();
            if (string.IsNullOrWhiteSpace(term)) return users;
            term = term.Trim();
            return users
                .Where(u => u.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            u.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public bool Delete(int id)
        {
            return repo.Delete(id);
        }

        public UserDTO? GetByEmail(string? email)
        {
            var data = repo.GetByEmail(email);
            return data == null ? null : mapper.Map<UserDTO>(data);
        }

        public Dictionary<int, string> GetNames(IEnumerable<int> ids)
        {
            return repo.GetNames(ids);
        }
    }
}
