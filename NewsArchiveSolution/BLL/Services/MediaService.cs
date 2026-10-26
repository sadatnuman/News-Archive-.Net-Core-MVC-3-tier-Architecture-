using BLL.DTOs;
using BLL.Interfaces;
using BLL.Media;

namespace BLL.Services
{
    public class MediaService : IMediaService
    {
        private const int MinImageBytes = 64;

        private readonly IFileStorage storage;

        public MediaService(IFileStorage storage)
        {
            this.storage = storage;
        }

        public MediaUploadResultDTO SaveArticleImage(Stream content, long length)
        {
            var now = DateTime.UtcNow;
            return Save(content, length, MediaLimits.MaxArticleImageBytes, $"uploads/articles/{now:yyyy}/{now:MM}");
        }

        public MediaUploadResultDTO SaveAvatar(Stream content, long length)
        {
            return Save(content, length, MediaLimits.MaxAvatarBytes, "uploads/avatars");
        }

        public bool IsUploaded(string? path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && path.StartsWith(MediaLimits.UploadPrefix, StringComparison.OrdinalIgnoreCase)
                && !path.Contains("..", StringComparison.Ordinal);
        }

        public void DeleteIfUploaded(string? path)
        {
            if (IsUploaded(path))
                storage.Delete(path!.TrimStart('/'));
        }

        private MediaUploadResultDTO Save(Stream content, long length, long maxBytes, string folder)
        {
            if (length <= 0) return MediaUploadResultDTO.Fail("Choose an image to upload.");
            if (length > maxBytes) return MediaUploadResultDTO.Fail(maxBytes == MediaLimits.MaxAvatarBytes
                ? "The image is too large. The limit is 2 MB."
                : "The image is too large. The limit is 5 MB.");

            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = content.Read(chunk, 0, chunk.Length)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > maxBytes)
                    return MediaUploadResultDTO.Fail(maxBytes == MediaLimits.MaxAvatarBytes
                        ? "The image is too large. The limit is 2 MB."
                        : "The image is too large. The limit is 5 MB.");
            }

            if (buffer.Length < MinImageBytes) return MediaUploadResultDTO.Fail("Only JPEG, PNG, WebP and GIF images can be uploaded.");

            var extension = DetectExtension(buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, 16)));
            if (extension == null) return MediaUploadResultDTO.Fail("Only JPEG, PNG, WebP and GIF images can be uploaded.");

            var relativePath = $"{folder}/{Guid.NewGuid():N}{extension}";
            buffer.Position = 0;
            storage.Save(buffer, relativePath);
            return MediaUploadResultDTO.Ok("/" + relativePath);
        }

        private static string? DetectExtension(ReadOnlySpan<byte> header)
        {
            if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return ".jpg";
            if (header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                return ".png";
            if (header.Length >= 6 && (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)))
                return ".gif";
            if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
                return ".webp";
            return null;
        }
    }
}
