using BLL.DTOs;
using BLL.Interfaces;
using DAL;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository repo;
        private readonly IMapper mapper;

        public CommentService(IDataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetCommentRepository();
            this.mapper = mapper;
        }

        public List<CommentDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<CommentDTO>>(data);
            return res;
        }

        public CommentDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<CommentDTO>(data);
            return res;
        }

        public bool Create(CommentDTO c)
        {
            c.Text = c.Text?.Trim() ?? string.Empty;
            if (c.Text.Length < 2 || c.Text.Length > 500) return false;
            var converted = mapper.Map<Comment>(c);
            return repo.Create(converted);
        }

        public bool Update(CommentDTO c)
        {
            var converted = mapper.Map<Comment>(c);
            return repo.Update(converted);
        }

        public bool Delete(int id)
        {
            return repo.Delete(id);
        }

        public List<CommentDTO> GetByArticleId(int articleId)
        {
            var data = repo.GetByArticleId(articleId);
            var res = mapper.Map<List<CommentDTO>>(data);
            return res;
        }
    }
}
