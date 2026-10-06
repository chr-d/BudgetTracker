using BudgetTracker.Events;

namespace BudgetTracker.Services;

public class LoggerService
{
    private readonly string _logFilePath;

    public LoggerService(TransactionService transactionService, string logsDirectory)
    {
        Directory.CreateDirectory(logsDirectory);
        _logFilePath = Path.Combine(logsDirectory, "transactions.log");
        transactionService.TransactionAdded += OnTransactionAdded;
    }

    private void OnTransactionAdded(object? sender, TransactionAddedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
