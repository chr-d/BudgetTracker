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
var removed = storageService.RemoveById(Guid.Parse("26683a58-6e82-437f-b0f1-a49e78a5fe17"));
Console.WriteLine(removed);
