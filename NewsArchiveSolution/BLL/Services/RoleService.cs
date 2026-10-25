using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class RoleService
    {
        private readonly IRepository<Role> repo;
        private readonly IMapper mapper;

        public RoleService(DataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetRoleRepository();
            this.mapper = mapper;
        }


        public List<RoleDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<RoleDTO>>(data);
            return res;
        }

        public RoleDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<RoleDTO>(data);
            return res;
        }

        public bool Create(RoleDTO c)
        {
            var converted = mapper.Map<Role>(c);
            return repo.Create(converted);
        }

        public bool Update(RoleDTO c)
        {
            var converted = mapper.Map<Role>(c);
            return repo.Update(converted);
        }

        public bool Delete(int id)
        {
            return repo.Delete(id);
        }
    }
}
