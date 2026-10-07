using BudgetTracker.Services;
using Spectre.Console;

namespace BudgetTracker.UI;

public class ConsoleUI(IAnsiConsole console, TransactionService transactionService)
{
    private readonly IAnsiConsole _console = console;
    private readonly TransactionService _transactionService = transactionService;

    private enum MenuAction
    {
        AddTransaction,
        RemoveTransaction,
        RangeReport,
        Exit,
    }

    public void Run()
    {
        while (true)
        {
            MenuAction choice;

            SelectionPrompt<MenuAction> prompt = new()
            {
                Title =
                    "[bold cyan]What would you like to do?[/]\n[grey](↑/↓ to choose, Enter to select)[/]",
            };
            prompt.UseConverter(ActionLabel);

            foreach (MenuAction action in Enum.GetValues<MenuAction>())
            {
                prompt.AddChoice(action);
            }

            choice = _console.Prompt(prompt);

            if (choice == MenuAction.Exit)
            {
                break;
            }

            try
            {
                switch (choice)
                {
                    case MenuAction.AddTransaction:
                        AddTransaction();
                        break;
                    case MenuAction.RemoveTransaction:
                        RemoveTransaction();
                        break;
                    case MenuAction.RangeReport:
                        RangeReport();
                        break;
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        _console.MarkupLine("[bold cyan]Goodbye - Thanks for using the app![/]");
    }

    // Menu actions
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

    // Menu helpers
    private static string ActionLabel(MenuAction action) =>
        action switch
        {
            MenuAction.AddTransaction => "Add transaction",
            MenuAction.RemoveTransaction => "Remove transaction",
            MenuAction.RangeReport => "Generate Report",
            _ => "Exit",
        };

    // Style helpers
    private void Error(string text) => _console.MarkupLine($"[red]Error:[/] {Markup.Escape(text)}");
}
