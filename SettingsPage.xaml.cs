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
        RebuildHistoryRangeOptions();
    }

    // Rok szkolny w Polsce zaczyna się 1 września.
    private static int SchoolYearOf(DateTime date)
        => date.Month >= 9 ? date.Year : date.Year - 1;

    private static string SchoolYearLabel(int year) => $"Rok szkolny {year}/{year + 1}";

    private void RebuildHistoryRangeOptions()
    {
        var years = _store.Data.Scans
            .Select(s => SchoolYearOf(s.Timestamp))
            .Distinct()
            .OrderByDescending(y => y)
            .Select(y => new HistoryRangeOption(SchoolYearLabel(y), y))
            .ToList();

        var options = new List<HistoryRangeOption> { new("Cała historia", null) }.Concat(years).ToList();
        HistoryRangePicker.ItemsSource = options;
        HistoryRangePicker.ItemDisplayBinding = new Binding(nameof(HistoryRangeOption.Label));
        HistoryRangePicker.SelectedIndex = options.Count > 0 ? 0 : -1;
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

    private async void OnDeleteHistoryRangeClicked(object sender, EventArgs e)
    {
        HideHints();
        SavedHint.IsVisible = false;

        var option = HistoryRangePicker.SelectedItem as HistoryRangeOption;
        if (option is null)
            return;

        var toDelete = _store.Data.Scans
            .Where(s => option.SchoolYear is null || SchoolYearOf(s.Timestamp) == option.SchoolYear)
            .ToList();

        if (toDelete.Count == 0)
        {
            HistoryHint.Text = "Brak skanów w wybranym okresie.";
            HistoryHint.TextColor = NeutralColor;
            HistoryHint.IsVisible = true;
            return;
        }

        var rangeLabel = option.SchoolYear is null
            ? "całej historii"
            : SchoolYearLabel(option.SchoolYear.Value).ToLowerInvariant();

        var confirm = await DisplayAlert("Usunąć historię?",
            $"Usunąć {toDelete.Count} skanów z {rangeLabel}? Operacji nie można cofnąć.",
            "Usuń", "Anuluj");
        if (!confirm)
            return;

        foreach (var scan in toDelete)
            _store.Data.Scans.Remove(scan);
        _store.Save();

        HistoryHint.Text = $"Usunięto {toDelete.Count} skanów.";
        HistoryHint.TextColor = SuccessColor;
        HistoryHint.IsVisible = true;
        RebuildHistoryRangeOptions();
    }

    private void HideHints()
    {
        DocsHint.IsVisible = false;
        ImportHint.IsVisible = false;
        ClearHint.IsVisible = false;
        HistoryHint.IsVisible = false;
    }

    private sealed record HistoryRangeOption(string Label, int? SchoolYear);
}