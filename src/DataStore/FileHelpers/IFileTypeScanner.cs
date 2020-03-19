namespace DataStore.FileHelpers
{
    public interface IFileTypeScanner
    {
        string ExtensionFromFile(string path);
        (bool FileTypeIsOk, string FileType) CheckFileType(string path);
    }
}