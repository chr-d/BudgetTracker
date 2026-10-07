using BudgetTracker.Models;
using BudgetTracker.Services;

StorageService storageService = new("data");
TransactionService transactionService = new(storageService);
LoggerService loggerService = new(transactionService, "logs");

Transaction transaction = new(
    Guid.NewGuid(),
    DateTimeOffset.Now,
    TransactionType.Income,
    "Test",
    9.99m
);

var newTransaction = transactionService.Add(TransactionType.Income, "Test", 9.99m);

Console.WriteLine($"Transaction added with ID: {newTransaction.Id}");

var removed = transactionService.Remove(Guid.Parse("346611c3-30a6-48f4-acc4-349c179468aa"));
Console.WriteLine(removed);

var transactions = transactionService.Query(new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9));

foreach (var t in transactions)
{
    Console.WriteLine(
        $"{t.Timestamp:yyyy-MM-dd HH:mm} [{t.Type}] {t.Description} {t.Amount} ({t.Id})"
    );
}
