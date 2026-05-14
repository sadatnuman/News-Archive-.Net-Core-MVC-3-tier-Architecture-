using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL.Repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class TagService
    {
        TagRepo repo;
        Mapper mapper;
        public TagService(TagRepo repo)
        {
            this.repo = repo;
            mapper = MapperConfig.GetMapper();
        }
        public List<TagDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<TagDTO>>(data);
            return res;

        }
        public TagDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<TagDTO>(data);
            return res;

        }
        public bool Create(TagDTO c)
        {
            var converted = mapper.Map<Tag>(c);
            return repo.Create(converted);
        }

        public bool Update(TagDTO c)
        {
            var converted = mapper.Map<Tag>(c);
            return repo.Update(converted);
        }
        public bool Delete(int id)
        {
            return repo.Delete(id);
        }
    }
}
