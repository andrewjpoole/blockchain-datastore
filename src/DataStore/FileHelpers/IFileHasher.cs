namespace DataStore.FileHelpers
{
    public interface IFileHasher
    {
        string CalculateHash(string filePath);
    }
}