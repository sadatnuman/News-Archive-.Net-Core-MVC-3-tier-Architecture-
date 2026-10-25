namespace BLL.Media
{
    public interface IFileStorage
    {
        void Save(Stream content, string relativePath);

        bool Delete(string relativePath);
    }
}
