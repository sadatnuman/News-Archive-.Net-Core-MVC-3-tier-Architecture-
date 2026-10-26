using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ICommentService
    {
        List<CommentDTO> Get();

        CommentDTO Get(int id);

        bool Create(CommentDTO c);

        bool Update(CommentDTO c);

        bool Delete(int id);

        List<CommentDTO> GetByArticleId(int articleId);
    }
}
