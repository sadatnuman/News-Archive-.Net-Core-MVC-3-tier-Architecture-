using BLL.DTOs;
using BLL.Interfaces;
using DAL;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> repo;
        private readonly IMapper mapper;

        public RoleService(IDataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetRoleRepository();
            this.mapper = mapper;
        }

        public List<RoleDTO> Get()
        {
            return mapper.Map<List<RoleDTO>>(repo.Get());
        }

        public RoleDTO? Get(int id)
        {
            var data = repo.Get(id);
            return data == null ? null : mapper.Map<RoleDTO>(data);
        }
    }
}
