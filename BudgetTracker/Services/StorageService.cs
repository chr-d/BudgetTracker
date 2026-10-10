using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class StorageService : IStorageService
{
    private readonly string _dataDirectory;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public StorageService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        Directory.CreateDirectory(_dataDirectory);
    }

    public void SaveTransaction(Transaction transaction)
    {
        string path = Path.Combine(
            _dataDirectory,
            transaction.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".json"
        );

        List<Transaction> transactions = File.Exists(path) ? ReadFile(path) : [];

        transactions.Add(transaction);
        WriteFile(path, transactions);
    }

    public IReadOnlyList<Transaction> LoadRange(DateOnly from, DateOnly to)
    {
        try
        {
            return Directory
                .EnumerateFiles(_dataDirectory, "????-??-??.json")
                .Where(path =>
                    DateOnly.TryParseExact(
                        Path.GetFileNameWithoutExtension(path),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateOnly day
                    )
                    && day >= from
                    && day <= to
                )
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(ReadFile)
                .SelectMany(transactions => transactions)
                .ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException(
                _dataDirectory,
                $"Could not read '{_dataDirectory}' directory.",
                ex
            );
        }
    }

    public bool RemoveById(Guid id)
    {
        string[] filePaths = [];
        try
        {
            filePaths = Directory.GetFiles(_dataDirectory, "????-??-??.json");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException(
                _dataDirectory,
                $"Could not read '{_dataDirectory}' directory.",
                ex
            );
        }
        foreach (string path in filePaths)
        {
            List<Transaction> transactions;

            try
            {
                transactions = ReadFile(path);
            }
            catch (StorageException ex)
            {
                Console.WriteLine(
                    $"Warning: Skipping file '{path}'. Reason: {ex.GetType().Name}: {ex.Message}"
                );
                continue;
            }

            Transaction? match = transactions.FirstOrDefault(t => t.Id == id);

            if (match is null)
            {
                continue;
            }

            transactions.Remove(match);
            WriteFile(path, transactions);
            return true;
        }

        return false;
    }

    private static List<Transaction> ReadFile(string path)
    {
        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException(path, $"Could not read '{path}'.", ex);
        }

        try
        {
            return JsonSerializer.Deserialize<List<Transaction>>(json, _jsonOptions) ?? [];
        }
        catch (JsonException ex)
        {
            throw new StorageException(
                path,
                $"'{Path.GetFileName(path)}' is damaged — fix it manually or delete it.",
                ex
            );
        }
    }

    private static void WriteFile(string path, List<Transaction> transactions)
    {
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(transactions, _jsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException(path, $"Could not write '{path}'.", ex);
        }
    }
}
