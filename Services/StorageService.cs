using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class StorageService
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

    public IEnumerable<Transaction> LoadRange(DateOnly from, DateOnly to)
    {
        throw new NotImplementedException();
    }

    public bool RemoveById(Guid id)
    {
        foreach (string path in Directory.GetFiles(_dataDirectory, "????-??-??.json"))
        {
            List<Transaction> transactions;

            try
            {
                transactions = ReadFile(path);
            }
            catch (Exception ex)
                when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
            {
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
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Transaction>>(json, _jsonOptions) ?? [];
    }

    private static void WriteFile(string path, List<Transaction> transactions)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(transactions, _jsonOptions));
    }
}
