using BudgetTracker.Services;
using BudgetTracker.UI;
using Spectre.Console;

StorageService storageService = new("data");
TransactionService transactionService = new(storageService);
LoggerService loggerService = new(transactionService, "logs");

new ConsoleUI(AnsiConsole.Console, transactionService).Run();
