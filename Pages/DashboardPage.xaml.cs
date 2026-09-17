using System.Collections.ObjectModel;

namespace TabletTracker.Pages;

public partial class DashboardPage : ContentPage
{
    public ObservableCollection<RecentScan> RecentScans { get; } = new()
    {
        new RecentScan("10:18", "STAN. 07", "Javonster Javowy - 5TP 1"),
        new RecentScan("10:11", "STAN. 03", "Fonk Skibididonk - 5TP 1"),
    };

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnScanTileTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Scan");
    private async void OnClassesTileTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Classes");
    private async void OnHistoryTileTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//History");
}

public record RecentScan(string Time, string Station, string StudentAndClass);