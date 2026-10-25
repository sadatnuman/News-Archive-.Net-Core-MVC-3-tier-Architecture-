namespace BLL.DTOs
{
    public class MediaUploadResultDTO
    {
        public bool Success { get; set; }

        public string? Path { get; set; }

        public string? Error { get; set; }

        public static MediaUploadResultDTO Ok(string path) => new() { Success = true, Path = path };

        public static MediaUploadResultDTO Fail(string error) => new() { Success = false, Error = error };
    }
}
