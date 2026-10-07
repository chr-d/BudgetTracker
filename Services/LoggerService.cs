using BudgetTracker.Events;
using BudgetTracker.Models;

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
        Transaction transaction = e.Transaction;
        string log =
            $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} | Added | {transaction.Type} | {transaction.Description} | {transaction.Amount:F2} | {transaction.Id}";

        try
        {
            File.AppendAllText(_logFilePath, log + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Warning: could not write to {_logFilePath}: {ex.Message}");
        }
    }
}
