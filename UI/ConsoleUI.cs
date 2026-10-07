using System.Globalization;
using BudgetTracker.Models;
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
                _console.Write(new Rule());
            }
        }

        _console.MarkupLine("[bold cyan]Goodbye - Thanks for using the app![/]");
    }

    // Menu actions
    private void AddTransaction()
    {
        _console.Write(new Rule("[bold cyan]Add a transaction[/]"));

        TransactionType? type = PromptType();
        if (type is null)
        {
            Cancelled();
            _console.Write(new Rule());
            return;
        }

        string? description = PromptDescription();
        if (description is null)
        {
            Cancelled();
            _console.Write(new Rule());
            return;
        }

        decimal? amount = PromptAmount();
        if (amount is null)
        {
            Cancelled();
            _console.Write(new Rule());
            return;
        }

        Transaction added = _transactionService.Add(type.Value, description, amount.Value);

        _console.MarkupLine(
            $"[green]Added[/] {added.Type} {(added.Type is TransactionType.Income ? "[green]" : "[red]")}{Money(added.Amount)}[/] - {Markup.Escape(added.Description)}"
        );
        _console.Write(new Rule());
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
    private void Warn(string text) => _console.MarkupLine($"[yellow]{Markup.Escape(text)}[/]");

    private void Error(string text) => _console.MarkupLine($"[red]Error:[/] {Markup.Escape(text)}");

    private void Cancelled() => Warn("Cancelled by user - returning to the menu.");

    // Format helpers
    private static string Money(decimal amount) =>
        string.Create(CultureInfo.InvariantCulture, $"{amount:F2} €");

    // Prompt validation helpers
    private TransactionType? PromptType()
    {
        SelectionPrompt<string> prompt = new() { Title = "[yellow]Type[/]" };
        prompt.AddChoices(["Income", "Expense", "Cancel"]);

        return _console.Prompt(prompt) switch
        {
            "Income" => TransactionType.Income,
            "Expense" => TransactionType.Expense,
            _ => null,
        };
    }

    private string? PromptDescription()
    {
        string input = _console.Prompt(
            new TextPrompt<string>("[yellow]Description[/]:").AllowEmpty()
        );

        return input.Length == 0 ? null : input.Trim();
    }

    private decimal? PromptAmount()
    {
        string input = _console.Prompt(
            new TextPrompt<string>("[yellow]Amount[/]:")
                .AllowEmpty()
                .Validate(s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return ValidationResult.Success();
                    }

                    string normalized = s.Replace(',', '.');

                    return
                        decimal.TryParse(
                            normalized,
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out decimal value
                        )
                        && value > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(
                            "[red]Enter a positive number (e.g. 2500, 10.99 or 10,99).[/]"
                        );
                })
        );

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string normalizedInput = input.Replace(',', '.');
        if (
            decimal.TryParse(
                normalizedInput,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal result
            )
        )
        {
            return result;
        }

        return null;
    }
}
