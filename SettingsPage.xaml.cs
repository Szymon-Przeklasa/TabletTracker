using Microsoft.Maui.Storage;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class SettingsPage : ContentPage
{
    private static readonly Color SuccessColor = Color.FromArgb("#2E9E6D");
    private static readonly Color NeutralColor = Color.FromArgb("#727F8C");
    private static readonly Color ErrorColor = Color.FromArgb("#B3261E");

    private readonly DataStore _store = DataStore.Instance;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TeacherNameEntry.Text = _store.Data.Settings.TeacherName;
        SavedHint.IsVisible = false;
    }

    private void OnSaveTeacherClicked(object sender, EventArgs e)
    {
        _store.Data.Settings.TeacherName = TeacherNameEntry.Text?.Trim() ?? "";
        _store.Save();
        SavedHint.IsVisible = true;
    }

    private async void OnSaveToDocumentsClicked(object sender, EventArgs e)
    {
        HideHints();
        SavedHint.IsVisible = false;

        var picked = await DocumentsService.PickDocumentsTargetAsync();
        if (picked)
        {
            _store.Save();
            DocsHint.Text = "Zapisano do Dokumentów.";
            DocsHint.TextColor = SuccessColor;
        }
        else
        {
            DocsHint.Text = "Zapis do Dokumentów jest dostępny na urządzeniach Android.";
            DocsHint.TextColor = NeutralColor;
        }
        DocsHint.IsVisible = true;
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        HideHints();
        SavedHint.IsVisible = false;

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Wybierz plik danych (.json)",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/json", "text/plain" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.iOS, new[] { "public.json", "public.text" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.json", "public.text" } },
                }),
            });
            if (result is null)
                return;

            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var ok = _store.ImportData(json);
            ImportHint.Text = ok ? "Dane zostały zaimportowane." : "Nie udało się odczytać pliku.";
            ImportHint.TextColor = ok ? SuccessColor : ErrorColor;
            ImportHint.IsVisible = true;
        }
        catch
        {
            ImportHint.Text = "Nie udało się odczytać pliku.";
            ImportHint.TextColor = ErrorColor;
            ImportHint.IsVisible = true;
        }
    }

    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        HideHints();
        SavedHint.IsVisible = false;

        var confirm = await DisplayAlert("Wyczyścić wszystkie dane?",
            "Spowoduje to usunięcie klas, uczniów, historii skanów i ustawień. Stanowiska zostaną utworzone od nowa (01–17).",
            "Wyczyść", "Anuluj");
        if (!confirm)
            return;

        _store.ResetData();
        ClearHint.Text = "Dane zostały wyczyszczone.";
        ClearHint.TextColor = SuccessColor;
        ClearHint.IsVisible = true;
    }

    private void HideHints()
    {
        DocsHint.IsVisible = false;
        ImportHint.IsVisible = false;
        ClearHint.IsVisible = false;
    }
}