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
        _console.Clear();
        ShowIntro();
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

        _console.MarkupLine(
            "[bold cyan]Goodbye - Thanks for using[/] [bold yellow]Coin Counter![/]"
        );
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
        _console.Write(new Rule("[bold cyan]Remove a transaction[/]"));

        string input = _console.Prompt(
            new TextPrompt<string>("[yellow]Transaction ID[/]:")
                .AllowEmpty()
                .Validate(s =>
                    string.IsNullOrWhiteSpace(s) || Guid.TryParse(s, out Guid result)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]That is not a valid ID.[/]")
                )
        );

        if (string.IsNullOrWhiteSpace(input))
        {
            Cancelled();
            _console.Write(new Rule());
            return;
        }

        Guid id = Guid.Parse(input);

        if (_transactionService.Remove(id))
        {
            _console.MarkupLine($"[green]Removed[/] transaction [grey]{id}[/].");
            _console.Write(new Rule());
        }
        else
        {
            Warn($"No transaction with ID {id} was found.");
            _console.Write(new Rule());
        }
    }

    private void RangeReport()
    {
        _console.Write(new Rule("[bold cyan]Report for a date range[/]"));

        if (PromptDateRange() is not { } range)
        {
            Cancelled();
            _console.Write(new Rule());
            return;
        }

        (DateOnly from, DateOnly to) = range;

        List<Transaction> rows = _transactionService.Query(from, to).ToList();

        if (rows.Count == 0)
        {
            Warn("No transactions found in this range.");
            _console.Write(new Rule());
            return;
        }

        string title = $"Transactions from {Day(from)} to {Day(to)}";

        _console.WriteLine();
        _console.Write(BuildTransactionTable(rows, title));
        ShowTotals(rows);
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

    private void ShowIntro()
    {
        _console.Write(
            new FigletText("Coin Counter") { Justification = Justify.Center }.Color(Color.Yellow)
        );

        _console.Write(
            new Rule(
                "[bold yellow]Coin Counter[/] - Never let your bank account hit Game Over!"
            ).Centered()
        );
        _console.WriteLine();
    }

    // Style helpers
    private void Warn(string text) => _console.MarkupLine($"[yellow]{Markup.Escape(text)}[/]");

    private void Error(string text) => _console.MarkupLine($"[red]Error:[/] {Markup.Escape(text)}");

    private void Cancelled() => Warn("Cancelled by user - returning to the menu.");

    private Table BuildTransactionTable(IReadOnlyList<Transaction> rows, string? title)
    {
        Table table = new() { Border = TableBorder.Rounded, BorderStyle = new Style(Color.Cyan) };

        if (title is not null)
        {
            table.Title = new TableTitle($"[bold cyan]{Markup.Escape(title)}[/]");
        }

        table.AddColumn(new TableColumn("[bold cyan]Date[/]"));
        table.AddColumn(new TableColumn("[bold cyan]Type[/]"));
        table.AddColumn(new TableColumn("[bold cyan]Description[/]"));
        table.AddColumn(new TableColumn("[bold cyan]Amount[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold cyan]Id[/]"));

        foreach (Transaction transaction in rows)
        {
            bool income = transaction.Type == TransactionType.Income;
            string color = income ? "green" : "red";

            table.AddRow(
                $"[grey]{DayAndTime(transaction.Timestamp)}[/]",
                $"[{color}]{transaction.Type}[/]",
                Markup.Escape(transaction.Description),
                $"[{color}]{Money(transaction.Amount)}[/]",
                $"[grey]{transaction.Id}[/]"
            );
        }

        return table;
    }

    private void ShowTotals(IReadOnlyList<Transaction> transactions)
    {
        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var balance = totalIncome - totalExpenses;

        string balanceColor = balance < 0 ? "red" : "green";

        _console.Write(new Rule("[grey]Totals[/]"));
        var table = new Table().HideHeaders().Border(TableBorder.None);
        table.AddColumn(new TableColumn(""));
        table.AddColumn(new TableColumn("").RightAligned());
        table.AddRow($"[bold]Total income[/]", $"[green]{Money(totalIncome)}[/]");
        table.AddRow($"[bold]Total expenses[/]", $"[red]{Money(totalExpenses)}[/]");
        table.AddRow(
            $"[bold]Balance[/]",
            string.Create(
                CultureInfo.InvariantCulture,
                $"[{balanceColor}]{(balance < 0 ? "-" : string.Empty)}{Math.Abs(balance):N2}€[/]"
            )
        );
        _console.Write(table);
        _console.Write(new Rule());
    }

    // Format helpers
    private static string Money(decimal amount) =>
        string.Create(CultureInfo.InvariantCulture, $"{amount:N2}€");

    private static string Day(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string DayAndTime(DateTimeOffset timestamp) =>
        timestamp.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    // Prompt validation helpers
    private TransactionType? PromptType()
    {
        SelectionPrompt<string> prompt = new() { Title = "[yellow]Type[/]" };

        prompt.AddChoices(["Income", "Expense", "Cancel"]);

        string selected = _console.Prompt(prompt);

        _console.MarkupLine($"[yellow]{prompt.Title}[/]: {Markup.Escape(selected)}");

        return selected switch
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

    private DateOnly? PromptDate(string label, DateOnly? earliest = null)
    {
        string input = _console.Prompt(
            new TextPrompt<string>($"[yellow]{label}[/] [grey](yyyy-MM-dd)[/]:")
                .AllowEmpty()
                .Validate(s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return ValidationResult.Success();
                    }

                    if (
                        !DateOnly.TryParseExact(
                            s,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateOnly date
                        )
                    )
                    {
                        return ValidationResult.Error(
                            "[red]Invalid date format (eg. 2026-12-31).[/]"
                        );
                    }

                    if (earliest is { } limit && date < limit)
                    {
                        return ValidationResult.Error(
                            "[red]The end date must be on or after the start date.[/]"
                        );
                    }

                    return ValidationResult.Success();
                })
        );

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return DateOnly.ParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private (DateOnly From, DateOnly To)? PromptDateRange()
    {
        DateOnly? from = PromptDate("Start date");
        if (from is null)
        {
            return null;
        }

        DateOnly? to = PromptDate("End date", earliest: from.Value);

        return to is null ? null : (from.Value, to.Value);
    }
}
