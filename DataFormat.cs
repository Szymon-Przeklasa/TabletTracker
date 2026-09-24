namespace TabletTracker;

public static class DataFormat
{
    public static string Station(string code)
        => code.All(char.IsDigit) ? $"STAN. {code}" : code;

    public static string Tablet(string id)
        => id.All(char.IsDigit) ? $"TABLET {id}" : id;
}