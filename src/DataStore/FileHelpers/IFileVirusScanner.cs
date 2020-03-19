namespace DataStore.FileHelpers
{
    public interface IFileVirusScanner
    {
        (bool FileContainsVirus, string Detail) ScanFile(string path);
    }
}