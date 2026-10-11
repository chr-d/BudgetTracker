using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.Tests;

public class StorageServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _path;
    private readonly StorageService _storage;

    public StorageServiceTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "BudgetTrackerTests",
            Guid.NewGuid().ToString()
        );
        _path = Path.Combine(_tempDir, "data");
        _storage = new StorageService(_path);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private static Transaction NewTransaction(
        DateTimeOffset timestamp,
        TransactionType type = TransactionType.Income,
        string description = "Salary",
        decimal amount = 2500m,
        Category category = Category.Salary
    ) => new(Guid.NewGuid(), timestamp, type, description, amount, category);

    [Fact]
    public void SaveTransaction_creates_a_file_named_after_the_transactions_day()
    {
        Transaction transaction = NewTransaction(new DateTime(2026, 10, 11));

        _storage.SaveTransaction(transaction);

        Assert.True(File.Exists(Path.Combine(_path, "2026-10-11.json")));
    }

    [Fact]
    public void LoadRange_returns_transactions_within_date_range()
    {
        Transaction before = NewTransaction(new DateTime(2026, 10, 10));
        Transaction first = NewTransaction(new DateTime(2026, 10, 11));
        Transaction last = NewTransaction(new DateTime(2026, 10, 12));
        Transaction after = NewTransaction(new DateTime(2026, 10, 13));

        foreach (Transaction transaction in new[] { before, first, last, after })
            _storage.SaveTransaction(transaction);

        List<Transaction> result = _storage
            .LoadRange(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 12))
            .ToList();

        Assert.Equal(new[] { first.Id, last.Id }, result.Select(t => t.Id));
    }

    [Fact]
    public void LoadRange_returns_empty_when_no_transactions_match()
    {
        _storage.SaveTransaction(NewTransaction(new DateTime(2026, 10, 11)));

        IReadOnlyList<Transaction> result = _storage.LoadRange(
            new DateOnly(2026, 10, 12),
            new DateOnly(2026, 10, 13)
        );

        Assert.Empty(result);
    }

    [Fact]
    public void SaveTransaction_appends_to_the_file_when_it_already_exists()
    {
        var date = new DateTime(2026, 10, 11);

        Transaction first = NewTransaction(date);
        Transaction second = NewTransaction(date);

        _storage.SaveTransaction(first);
        _storage.SaveTransaction(second);

        string path = Path.Combine(_path, "2026-10-11.json");

        Assert.True(File.Exists(path));

        List<Transaction> savedTransactions = _storage
            .LoadRange(DateOnly.FromDateTime(date), DateOnly.FromDateTime(date))
            .ToList();

        Assert.Equal(2, savedTransactions.Count);
        Assert.Equal(first.Id, savedTransactions[0].Id);
        Assert.Equal(second.Id, savedTransactions[1].Id);
    }

    [Fact]
    public void RemoveById_removes_matching_transaction_and_returns_true()
    {
        Transaction transaction = NewTransaction(new DateTime(2026, 10, 11));
        _storage.SaveTransaction(transaction);

        bool removed = _storage.RemoveById(transaction.Id);

        Assert.True(removed);
        Assert.Empty(_storage.LoadRange(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 11)));
    }

    [Fact]
    public void RemoveById_returns_false_when_id_does_not_exist()
    {
        bool removed = _storage.RemoveById(Guid.NewGuid());

        Assert.False(removed);
    }
}
