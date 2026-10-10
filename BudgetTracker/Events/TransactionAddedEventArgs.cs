using BudgetTracker.Models;

namespace BudgetTracker.Events;

public sealed class TransactionAddedEventArgs(Transaction transaction) : EventArgs
{
    public Transaction Transaction { get; } = transaction;
}
