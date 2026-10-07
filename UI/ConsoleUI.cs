using BudgetTracker.Services;
using Spectre.Console;

namespace BudgetTracker.UI;

public class ConsoleUI(IAnsiConsole console, TransactionService transactionService)
{
    private readonly IAnsiConsole _console = console;
    private readonly TransactionService _transactionService = transactionService;

    public void Run()
    {
        _console.MarkupLine("[italic underline red]Hello, World![/]");
    }

    private void AddTransaction()
    {
        throw new NotImplementedException();
    }

    private void RemoveTransaction()
    {
        throw new NotImplementedException();
    }

    private void RangeReport()
    {
        throw new NotImplementedException();
    }
}
