using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL.Repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class UserService
    {
        UserRepo repo;
        Mapper mapper;
        public UserService(UserRepo repo)
        {
            this.repo = repo;
            mapper = MapperConfig.GetMapper();
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
            return repo.Create(converted);
        }

        public bool Update(UserDTO c)
        {
            var converted = mapper.Map<User>(c);
            return repo.Update(converted);
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
