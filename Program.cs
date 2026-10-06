using BudgetTracker.Services;

StorageService storageService = new("data");
TransactionService transactionService = new(storageService);
LoggerService loggerService = new(transactionService, "logs");
