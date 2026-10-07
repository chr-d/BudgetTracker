using BudgetTracker.Events;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class TransactionService(StorageService storage)
{
    private readonly StorageService _storage = storage;

    public event EventHandler<TransactionAddedEventArgs>? TransactionAdded;

    private void OnTransactionAdded(TransactionAddedEventArgs e) =>
        TransactionAdded?.Invoke(this, e);

    public Transaction Add(TransactionType type, string description, decimal amount)
    {
        throw new NotImplementedException();
    }

    public bool Remove(Guid id) => _storage.RemoveById(id);

    public IEnumerable<Transaction> Query(DateOnly from, DateOnly to) =>
        _storage.LoadRange(from, to).OrderBy(t => t.Timestamp);
}
