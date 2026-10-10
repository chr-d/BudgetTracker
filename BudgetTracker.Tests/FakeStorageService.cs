using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.Tests;

public sealed class FakeStorageService : IStorageService
{
    public List<Transaction> Saved { get; } = [];

    public void SaveTransaction(Transaction transaction) => Saved.Add(transaction);

    public IReadOnlyList<Transaction> LoadRange(DateOnly from, DateOnly to) => Saved;

    public bool RemoveById(Guid id)
    {
        var match = Saved.FirstOrDefault(t => t.Id == id);

        if (match is null)
        {
            return false;
        }

        Saved.Remove(match);
        return true;
    }
}
