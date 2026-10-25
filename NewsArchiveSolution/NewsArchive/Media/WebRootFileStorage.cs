using BLL.Media;

namespace NewsArchive.Media
{
    public class WebRootFileStorage : IFileStorage
    {
        private readonly string root;

        public WebRootFileStorage(IWebHostEnvironment environment)
        {
            root = Path.GetFullPath(environment.WebRootPath);
        }

        public void Save(Stream content, string relativePath)
        {
            var target = Resolve(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            using var file = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            content.CopyTo(file);
        }

        public bool Delete(string relativePath)
        {
            var target = Resolve(relativePath);
            if (!File.Exists(target)) return false;
            File.Delete(target);
            return true;
        }

        private string Resolve(string relativePath)
        {
            var uploads = Path.Combine(root, "uploads") + Path.DirectorySeparatorChar;
            var target = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!target.StartsWith(uploads, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Files can only be stored under wwwroot/uploads.");
            return target;
        }
    }
}
