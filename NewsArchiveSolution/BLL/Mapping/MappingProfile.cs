using AutoMapper;
using BLL.DTOs;
using BLL.Security;
using DAL.EF.Tables;

namespace BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Article, ArticleDTO>();
            CreateMap<ArticleDTO, Article>().ForMember(d => d.Tags, o => o.Ignore());
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Comment, CommentDTO>().ReverseMap();
            CreateMap<Tag, TagDTO>().ForMember(d => d.ArticleCount, o => o.Ignore());
            CreateMap<TagDTO, Tag>().ForMember(d => d.Articles, o => o.Ignore());
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Role, RoleDTO>().ReverseMap();
            CreateMap<User, AuthUserDTO>()
                .ForMember(d => d.RoleName, o => o.MapFrom(s => AppRoles.Name(s.RoleId)))
                .ForMember(d => d.SecurityStamp, o => o.MapFrom(s => PasswordHasher.Stamp(s.PasswordHash)));
        }
    }
}
