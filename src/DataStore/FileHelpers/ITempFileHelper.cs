using System.IO;

namespace DataStore.FileHelpers
{
    public interface ITempFileHelper
    {
        FileStream Create(string path);
        
        void Delete(string filePath);
    }
}