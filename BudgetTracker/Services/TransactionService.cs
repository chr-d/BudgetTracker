using BudgetTracker.Events;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class TransactionService(StorageService storage)
{
    private readonly StorageService _storage = storage;

    public event EventHandler<TransactionAddedEventArgs>? TransactionAdded;

    private void OnTransactionAdded(TransactionAddedEventArgs e) =>
        TransactionAdded?.Invoke(this, e);

    public Transaction Add(
        TransactionType type,
        string description,
        decimal amount,
        Category category
    )
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentNullException(
                nameof(description),
                "The description cannot be empty."
            );
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "The amount must be greater than zero."
            );
        }

        Transaction transaction = new(
            Id: Guid.NewGuid(),
            Timestamp: DateTimeOffset.Now,
            Type: type,
            Description: description.Trim(),
            Amount: amount,
            Category: category
        );

        _storage.SaveTransaction(transaction);
        OnTransactionAdded(new TransactionAddedEventArgs(transaction));

        return transaction;
    }

    public bool Remove(Guid id) => _storage.RemoveById(id);

    public IEnumerable<Transaction> Query(DateOnly from, DateOnly to) =>
        _storage.LoadRange(from, to).OrderBy(t => t.Timestamp);
}
