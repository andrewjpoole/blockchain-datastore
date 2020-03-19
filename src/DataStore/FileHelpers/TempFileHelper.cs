using System.IO;

namespace DataStore.FileHelpers
{
    public class TempFileHelper : ITempFileHelper
    {
        public FileStream Create(string path)
        {
            return File.Create(path);
        }

        public void Delete(string filePath)
        {
            File.Delete(filePath);
        }
    }
}