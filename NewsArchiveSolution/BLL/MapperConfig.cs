using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;

namespace BLL
{
    public class MapperConfig
    {
        static MapperConfiguration config = new MapperConfiguration(cfg => {

            cfg.CreateMap<Article, ArticleDTO>().ReverseMap();
            cfg.CreateMap<Category, CategoryDTO>().ReverseMap();
            cfg.CreateMap<Comment, CommentDTO>().ReverseMap();
            cfg.CreateMap<Tag, TagDTO>().ReverseMap();
            cfg.CreateMap<User, UserDTO>().ReverseMap();
            cfg.CreateMap<Role, RoleDTO>().ReverseMap();

        });

        public static Mapper GetMapper()
        {
            return new Mapper(config);
        }

    }
}
