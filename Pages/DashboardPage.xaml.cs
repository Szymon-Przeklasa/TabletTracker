using System.Collections.ObjectModel;
using System.Globalization;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly DataStore _store = DataStore.Instance;

    public ObservableCollection<RecentScan> RecentScans { get; } = new();

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }

    private void Refresh()
    {
        var now = DateTime.Now;
        var pl = new CultureInfo("pl-PL");

        DayLabel.Text = char.ToUpperInvariant(now.ToString("dddd", pl)[0]) + now.ToString("dddd", pl)[1..];
        DateLabel.Text = now.ToString("d MMMM yyyy", pl);
        TimeLabel.Text = now.ToString("HH:mm");

        var stations = _store.Data.Stations;
        StationCountLabel.Text = $"Liczba stanowisk: {stations.Count}";

        var assigned = stations.Count(s => s.StudentId is not null);
        ProgressTextLabel.Text = $"{assigned} / {stations.Count}";
        ProgressCaptionLabel.Text = assigned == 1
            ? "stanowisko przypisane"
            : "stanowisk przypisanych";
        ProgressBarControl.Progress = stations.Count == 0 ? 0 : (double)assigned / stations.Count;

        RecentScans.Clear();
        foreach (var scan in _store.Data.Scans
                     .OrderByDescending(s => s.Timestamp)
                     .Take(5))
        {
            RecentScans.Add(new RecentScan(
                scan.Timestamp.ToString("HH:mm"),
                DisplayStationCode(scan.StationCode),
                $"{scan.StudentName} — {scan.ClassName}"));
        }
    }

    private static string DisplayStationCode(string code) => code.All(char.IsDigit) ? $"STAN. {code}" : code;

    private async void OnScanTileTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Scan");
    private async void OnClassesTileTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Classes");
    private async void OnHistoryTileTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//History");
}

public record RecentScan(string Time, string Station, string StudentAndClass);