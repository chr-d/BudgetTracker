namespace BudgetTracker.Services;

public class StorageException(string filePath, string message, Exception innerException)
    : Exception(message, innerException)
{
    public string FilePath { get; } = filePath;
}
