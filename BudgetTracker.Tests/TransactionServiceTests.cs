using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.Tests;

public class TransactionServiceTests
{
    private readonly FakeStorageService _storage = new();
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService(_storage);
    }

    [Fact]
    public void Add_raises_TransactionAdded_once_with_the_right_transaction()
    {
        int raised = 0;
        Transaction? received = null;
        object? sender = null;
        _service.TransactionAdded += (s, e) =>
        {
            raised++;
            sender = s;
            received = e.Transaction;
        };

        Transaction added = _service.Add(TransactionType.Income, "Salary", 2500m, Category.Salary);

        Assert.Equal(1, raised);
        Assert.Same(_service, sender);
        Assert.Same(added, received);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Add_rejects_an_amount_of_zero_or_less(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.Add(TransactionType.Expense, "Rent", amount, Category.Housing)
        );

        Assert.Empty(_storage.Saved);
    }

    [Fact]
    public void Remove_returns_false_for_an_unknown_id()
    {
        bool removed = _service.Remove(Guid.NewGuid());

        Assert.False(removed);
    }
}
