using BudgetTracker.Models;

namespace BudgetTracker.Services;

public interface IStorageService
{
    void SaveTransaction(Transaction transaction);
    IReadOnlyList<Transaction> LoadRange(DateOnly from, DateOnly to);
    bool RemoveById(Guid id);
}
