using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IRoleService
    {
        List<RoleDTO> Get();

        RoleDTO Get(int id);

        bool Create(RoleDTO c);

        bool Update(RoleDTO c);

        bool Delete(int id);
    }
}
