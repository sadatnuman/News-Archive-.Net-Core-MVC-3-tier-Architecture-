using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IMediaService
    {
        MediaUploadResultDTO SaveArticleImage(Stream content, long length);

        MediaUploadResultDTO SaveAvatar(Stream content, long length);

        bool DeleteArticleImageIfUnused(string? path);

        bool DeleteAvatarIfUnused(string? path);
    }
}
