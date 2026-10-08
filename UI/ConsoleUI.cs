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

        Category? category = PromptCategory();
        if (category is null)
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

        Transaction added = _transactionService.Add(
            type.Value,
            description,
            amount.Value,
            category.Value
        );

        _console.MarkupLine(
            $"[green]Added[/] {added.Type} {(added.Type is TransactionType.Income ? "[green]" : "[red]")}{Money(added.Amount)}[/] - {Markup.Escape(added.Description)}"
        );
        _console.Write(new Rule());
    }

    private void RemoveTransaction()
    {
        _console.Write(new Rule("[bold cyan]Remove a transaction[/]"));

        Guid? id = PromptTransactionId();

        if (id is null)
        {
            Cancelled();
            _console.Write(new Rule());
            return;
        }

        if (_transactionService.Remove(id.Value))
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
        SelectionPrompt<string> prompt = new() { Title = "[cyan]Type[/]" };

        prompt.AddChoices([
            .. Enum.GetValues<TransactionType>().Select(type => type.ToString()),
            "Cancel",
        ]);

        string selected = _console.Prompt(prompt);

        _console.MarkupLine($"[cyan]{prompt.Title}[/]: {Markup.Escape(selected)}");

        return selected == "Cancel" ? null : Enum.Parse<TransactionType>(selected);
    }

    private Category? PromptCategory()
    {
        SelectionPrompt<string> prompt = new() { Title = "[cyan]Category[/]" };

        prompt.AddChoices([
            .. Enum.GetValues<Category>().Select(category => category.ToString()),
            "Cancel",
        ]);

        string selected = _console.Prompt(prompt);

        _console.MarkupLine($"[cyan]{prompt.Title}[/]: {Markup.Escape(selected)}");

        return selected == "Cancel" ? null : Enum.Parse<Category>(selected);
    }

    private string? PromptDescription()
    {
        string input = _console.Prompt(
            new TextPrompt<string>("[cyan]Description[/]:").AllowEmpty()
        );

        return input.Length == 0 ? null : input.Trim();
    }

    private decimal? PromptAmount()
    {
        decimal amount = 0m;

        string input = _console.Prompt(
            new TextPrompt<string>("[cyan]Amount[/]:")
                .AllowEmpty()
                .Validate(s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return ValidationResult.Success();
                    }

                    var isParsed =
                        decimal.TryParse(
                            // HACK Replace (',', '.') allows both chars to be used as decimal symbol
                            // but they can't be used as thousand seperator anymore
                            s.Replace(',', '.'),
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out amount
                        )
                        && amount > 0;

                    return isParsed
                        ? ValidationResult.Success()
                        : ValidationResult.Error(
                            "[red]Enter a positive number (e.g. 2500, 10.99 or 10,99).[/]"
                        );
                })
        );

        return string.IsNullOrWhiteSpace(input) ? null : amount;
    }

    private Guid? PromptTransactionId()
    {
        Guid id = Guid.Empty;

        string input = _console.Prompt(
            new TextPrompt<string>("[cyan]Transaction ID[/]:")
                .AllowEmpty()
                .Validate(s =>
                    string.IsNullOrWhiteSpace(s) || Guid.TryParse(s, out id)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]That is not a valid ID.[/]")
                )
        );

        return string.IsNullOrWhiteSpace(input) ? null : id;
    }

    private DateOnly? PromptDate(string label, DateOnly? earliest = null)
    {
        DateOnly date = default;

        string input = _console.Prompt(
            new TextPrompt<string>($"[cyan]{label}[/] [grey](yyyy-MM-dd)[/]:")
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
                            out date
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

        return string.IsNullOrWhiteSpace(input) ? null : date;
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
