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

// storageService.SaveTransaction(transaction);

// var removed = storageService.RemoveById(Guid.Parse("26683a58-6e82-437f-b0f1-a49e78a5fe17"));
// Console.WriteLine(removed);

var transactions = storageService.LoadRange(new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9));

foreach (var t in transactions)
{
    Console.WriteLine($"{t.Timestamp:yyyy-MM-dd} [{t.Type}] {t.Description} {t.Amount}");
}
