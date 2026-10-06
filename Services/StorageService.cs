using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class StorageService
{
    private readonly string _dataDirectory;

    public StorageService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        Directory.CreateDirectory(_dataDirectory);
    }

    public void SaveTransaction(Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Transaction> LoadRange(DateOnly from, DateOnly to)
    {
        throw new NotImplementedException();
    }

    public bool RemoveById(Guid id)
    {
        throw new NotImplementedException();
    }
}
