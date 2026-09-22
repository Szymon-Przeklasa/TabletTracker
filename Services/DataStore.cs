using System.Text.Json;
using Microsoft.Maui.Storage;
using TabletTracker.Models;

namespace TabletTracker.Services;

public sealed class DataStore
{
    private static readonly Lazy<DataStore> LazyInstance = new(() => new DataStore());
    public static DataStore Instance => LazyInstance.Value;

    private const string FileName = "tablettracker-data.json";
    private readonly string _filePath;
    private readonly object _saveLock = new();

    public AppData Data { get; }

    private DataStore()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
        Data = Load() ?? Seed();
    }

    public void Save()
    {
        lock (_saveLock)
        {
            var json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    private AppData? Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppData>(json);
        }
        catch
        {
            return null;
        }
    }

    private AppData Seed()
    {
        var data = new AppData();
        for (var i = 1; i <= 17; i++)
            data.Stations.Add(new Station { Code = i.ToString("00") });
        Save();
        return data;
    }

    public Station? FindStation(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return Data.Stations.FirstOrDefault(s =>
            string.Equals(s.Code.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public Student? FindStudent(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return null;

        return Data.Classes.SelectMany(c => c.Students)
            .FirstOrDefault(s => s.Id == studentId);
    }

    public StudentClass? FindClassOfStudent(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return null;

        return Data.Classes.FirstOrDefault(c => c.Students.Any(s => s.Id == studentId));
    }

    public string? StudentName(string? studentId) => FindStudent(studentId ?? "")?.FullName;

    public string? ClassNameOfStudent(string? studentId) => FindClassOfStudent(studentId ?? "")?.Name;

    public static string NormalizeCode(string code) => code?.Trim() ?? "";
}