using System.Collections.ObjectModel;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class ScanHistoryPage : ContentPage
{
    private readonly DataStore _store = DataStore.Instance;

    public ObservableCollection<HistoryEntry> Entries { get; } = new();

    private List<DateOption> _dateOptions = new();
    private List<ClassOption> _classOptions = new();

    public ScanHistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RebuildFilters();
        ApplyFilters();
    }

    private void RebuildFilters()
    {
        var scans = _store.Data.Scans;

        var currentDate = (DateFilterPicker.SelectedItem as DateOption)?.Value;
        _dateOptions = scans
            .OrderByDescending(s => s.Timestamp)
            .Select(s => s.Timestamp.Date)
            .Distinct()
            .Select(d => new DateOption(d, d.ToString("dd.MM.yyyy")))
            .ToList();
        DateFilterPicker.ItemsSource = new List<DateOption> { new(null, "Data: wszystkie") }.Concat(_dateOptions).ToList();
        DateFilterPicker.ItemDisplayBinding = new Binding(nameof(DateOption.Label));

        var currentClass = (ClassFilterPicker.SelectedItem as ClassOption)?.Value;
        _classOptions = scans
            .Select(s => s.ClassName)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .Select(c => new ClassOption(c, c))
            .ToList();
        ClassFilterPicker.ItemsSource = new List<ClassOption> { new(null, "Klasa: wszystkie") }.Concat(_classOptions).ToList();
        ClassFilterPicker.ItemDisplayBinding = new Binding(nameof(ClassOption.Label));

        if (currentDate is null)
            DateFilterPicker.SelectedIndex = 0;
        else
        {
            var idx = _dateOptions.FindIndex(o => o.Value == currentDate);
            DateFilterPicker.SelectedIndex = idx < 0 ? 0 : idx + 1;
        }

        if (currentClass is null)
            ClassFilterPicker.SelectedIndex = 0;
        else
        {
            var idx = _classOptions.FindIndex(o => o.Value == currentClass);
            ClassFilterPicker.SelectedIndex = idx < 0 ? 0 : idx + 1;
        }
    }

    private void ApplyFilters()
    {
        var selectedDate = (DateFilterPicker.SelectedItem as DateOption)?.Value;
        var selectedClass = (ClassFilterPicker.SelectedItem as ClassOption)?.Value;
        var search = SearchEntry.Text?.Trim() ?? "";

        IEnumerable<Models.ScanRecord> scans = _store.Data.Scans;

        if (selectedDate is not null)
            scans = scans.Where(s => s.Timestamp.Date == selectedDate);
        if (selectedClass is not null)
            scans = scans.Where(s => string.Equals(s.ClassName, selectedClass, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(search))
        {
            scans = scans.Where(s =>
                s.StudentName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || s.TabletId.Contains(search, StringComparison.OrdinalIgnoreCase)
                || s.StationCode.Contains(search, StringComparison.OrdinalIgnoreCase)
                || s.ClassName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = scans
            .OrderByDescending(s => s.Timestamp)
            .ToList();

        Entries.Clear();
        foreach (var scan in ordered)
        {
            Entries.Add(new HistoryEntry(
                scan.Timestamp.ToString("dd.MM"),
                scan.Timestamp.ToString("HH:mm"),
                DisplayStationCode(scan.StationCode),
                string.IsNullOrWhiteSpace(scan.TabletId) ? "—" : scan.TabletId,
                scan.StudentName,
                scan.ClassName,
                string.IsNullOrWhiteSpace(scan.TeacherName) ? "—" : scan.TeacherName));
        }

        SubtitleLabel.Text = ordered.Count == 1
            ? "1 zarejestrowany skan"
            : $"{ordered.Count} zarejestrowanych skanów";
    }

    private static string DisplayStationCode(string code) => code.All(char.IsDigit) ? $"STAN. {code}" : code;

    private void OnDateFilterChanged(object sender, EventArgs e) => ApplyFilters();
    private void OnClassFilterChanged(object sender, EventArgs e) => ApplyFilters();
    private void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private sealed record DateOption(DateTime? Value, string Label);
    private sealed record ClassOption(string? Value, string Label);
}

public record HistoryEntry(string Date, string Time, string Station, string Tablet, string Student, string Class, string Teacher);