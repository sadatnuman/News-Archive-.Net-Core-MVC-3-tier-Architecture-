using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IRoleService
    {
        List<RoleDTO> Get();

        RoleDTO? Get(int id);
    }
}
