# BudgetTracker

A console-based personal budget tracker built with .NET 10 and [Spectre.Console](https://spectreconsole.net/). Add and remove transactions and generate reports by date range. Transactions are persisted as daily JSON files, a separate log file is appended on every add.

## Project layout

```
BudgetTracker.slnx
├── BudgetTracker/                # App
│   ├── Program.cs                # Composition root
│   ├── Models/                   # Transaction, Category, TransactionType
│   ├── Events/                   # TransactionAddedEventArgs
│   ├── Services/                 # Storage / Transaction / Logger services, IStorageService, StorageException
│   └── UI/ConsoleUI.cs           # Menu and helpers
├── BudgetTracker.Tests/          # xUnit tests
└── docs/                         # Class diagram
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Build & run

All commands run from the solution root (where `BudgetTracker.slnx` lives).

| Action        | Command                              |
| ------------- | ------------------------------------ |
| Run the app   | `dotnet run --project BudgetTracker` |
| Run all tests | `dotnet test`                        |

On first run two folders are created next to the working directory:

- `data/` — one JSON file per day (`yyyy-MM-dd.json`), each containing an array of `Transaction`.
- `logs/transactions.log` — append-only audit log of every added transaction.

## Menu options

Select options with an arrow-key menu (`↑`/`↓` to move, `Enter` to select). Every input prompt accepts an empty value to **cancel** and return to the menu.

| Menu label                      | Action                         | What it does                                                                                                                                                                                               |
| ------------------------------- | ------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Add Transaction**             | `MenuAction.AddTransaction`    | Prompts for _Type_ (Income/Expense), _Category_, _Description_, _Amount_ (positive number, `,` or `.` decimal separator allowed), then persists via `TransactionService.Add` and fires `TransactionAdded`. |
| **Remove Transaction**          | `MenuAction.RemoveTransaction` | Prompts for a transaction `Guid`; removes it from the day's JSON file via `TransactionService.Remove`. Warns if not found.                                                                                 |
| **Generate Report**             | `MenuAction.RangeReport`       | Prompts for a _Start date_ and _End date_ (`yyyy-MM-dd`), then prints a table of every transaction in range plus Income / Expenses / Balance totals.                                                       |
| **Generate Category Breakdown** | `MenuAction.CategoryReport`    | Same date-range prompt, then prints a per-category income/expense/balance table, two Spectre.Console `BreakdownChart`s (income and expenses by category), and totals.                                      |
| **Exit**                        | `MenuAction.Exit`              | Leaves the menu loop and prints the goodbye banner.                                                                                                                                                        |

## Architecture

`Program.cs` wires the services:

```
StorageService("data")  ──►  TransactionService  ──►  ConsoleUI.Run()
                                  ▲
                                  │ subscribes
                          LoggerService("logs")
```

- `TransactionService` depends on `IStorageService`, so unit tests substitute `FakeStorageService` (no real filesystem).
- `StorageService` writes one JSON file per day; corrupted files surface as `StorageException`, which `ConsoleUI` catches.
- `LoggerService` subscribes to `TransactionService.TransactionAdded` and appends a line per add.
