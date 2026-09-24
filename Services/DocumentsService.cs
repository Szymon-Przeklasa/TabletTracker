using Microsoft.Maui.Storage;

namespace TabletTracker.Services;

/// <summary>
/// Zarządza zapisem danych do publicznego folderu Dokumenty na Androidzie
/// przez Storage Access Framework (utrwalone uprawnienie do wybranego URI).
/// </summary>
public static class DocumentsService
{
    private const string UriKey = "documents_uri";

    public static string? DocumentsUriString
        => Preferences.Default.Get<string?>(UriKey, null);

    public static bool HasDocumentsTarget
        => !string.IsNullOrWhiteSpace(DocumentsUriString);

    public static async Task<bool> PickDocumentsTargetAsync()
    {
#if ANDROID
        var uri = await MainActivity.CreateDocumentAsync("tablettracker-data.json", "application/json");
        if (uri is null)
            return false;

        Preferences.Default.Set(UriKey, uri.ToString()!);
        return true;
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    public static bool WriteToDocuments(string json)
    {
#if ANDROID
        var uriString = DocumentsUriString;
        if (string.IsNullOrWhiteSpace(uriString))
            return false;

        try
        {
            var uri = Android.Net.Uri.Parse(uriString);
            var resolver = Platform.CurrentActivity?.ContentResolver;
            if (uri is null || resolver is null)
                return false;

            using var stream = resolver.OpenOutputStream(uri, "wt");
            if (stream is null)
                return false;

            using var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            return true;
        }
        catch
        {
            return false;
        }
#else
        return false;
#endif
    }
}