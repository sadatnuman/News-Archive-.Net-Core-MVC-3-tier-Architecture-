using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class TagService
    {
        private readonly IRepository<Tag> repo;
        private readonly IMapper mapper;

        public TagService(DataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetTagRepository();
            this.mapper = mapper;
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
