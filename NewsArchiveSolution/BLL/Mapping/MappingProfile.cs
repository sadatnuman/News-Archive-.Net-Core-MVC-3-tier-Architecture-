using BLL.DTOs;
using BLL.Security;
using DAL.EF.Tables;
using AutoMapper;

namespace BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public const int MaxDepth = 3;

        public MappingProfile()
        {
            CreateMap<Article, ArticleDTO>().MaxDepth(MaxDepth);
            CreateMap<ArticleDTO, Article>().MaxDepth(MaxDepth).ForMember(d => d.Tags, o => o.Ignore());
            CreateMap<Category, CategoryDTO>().MaxDepth(MaxDepth).ReverseMap().MaxDepth(MaxDepth);
            CreateMap<Comment, CommentDTO>().MaxDepth(MaxDepth).ReverseMap().MaxDepth(MaxDepth);
            CreateMap<Tag, TagDTO>().MaxDepth(MaxDepth).ForMember(d => d.ArticleCount, o => o.Ignore());
            CreateMap<TagDTO, Tag>().MaxDepth(MaxDepth).ForMember(d => d.Articles, o => o.Ignore());
            CreateMap<User, UserDTO>().MaxDepth(MaxDepth).ReverseMap().MaxDepth(MaxDepth);
            CreateMap<Role, RoleDTO>().MaxDepth(MaxDepth).ReverseMap().MaxDepth(MaxDepth);
            CreateMap<User, AuthUserDTO>()
                .MaxDepth(MaxDepth)
                .ForMember(d => d.RoleName, o => o.MapFrom(s => AppRoles.Name(s.RoleId)))
                .ForMember(d => d.SecurityStamp, o => o.MapFrom(s => PasswordHasher.Stamp(s.PasswordHash)));
        }
    }
}
