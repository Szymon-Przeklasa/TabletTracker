using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using TabletTracker.Models;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class AssignStationPage : ContentPage, IQueryAttributable
{
    private readonly DataStore _store = DataStore.Instance;

    public ObservableCollection<StationItem> Stations { get; } = new();

    private string _tabletId = "";
    private string _filter = "All";
    private bool _navigating;

    public AssignStationPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var value) && value is string code && !string.IsNullOrWhiteSpace(code))
        {
            _tabletId = DataStore.NormalizeCode(code);
            TabletIdLabel.Text = DisplayTabletId(_tabletId);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshAll();
        Dispatcher.Dispatch(RefreshGridHeight);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        RefreshGridHeight();
    }

    private void RefreshGridHeight()
    {
        if (StationsGrid is null || Height <= 0 || StationsGrid.Y <= 0)
            return;
        StationsGrid.HeightRequest = Math.Max(120, Height - StationsGrid.Y - 20);
    }

    private void RefreshAll()
    {
        var data = _store.Data;
        Stations.Clear();

        foreach (var station in data.Stations)
        {
            var item = new StationItem(station.Code)
            {
                IsAssigned = station.StudentId is not null,
                TabletInfo = string.IsNullOrWhiteSpace(station.TabletId) ? "" : $"TABLET {station.TabletId}",
                StudentInfo = BuildStudentInfo(station),
            };

            var show = _filter == "All"
                || (_filter == "Assigned" && item.IsAssigned)
                || (_filter == "Free" && !item.IsAssigned);
            if (show)
                Stations.Add(item);
        }
    }

    private string BuildStudentInfo(Station station)
    {
        if (station.StudentId is null)
            return "Wolne";

        var name = _store.StudentName(station.StudentId);
        var cls = _store.ClassNameOfStudent(station.StudentId);
        return string.IsNullOrWhiteSpace(cls) ? name! : $"{name} · {cls}";
    }

    private async void OnStationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_navigating)
            return;

        if (e.CurrentSelection.FirstOrDefault() is not StationItem selected)
            return;

        _navigating = true;
        StationsGrid.SelectedItem = null;
        await Shell.Current.GoToAsync(
            $"AssignClass?code={Uri.EscapeDataString(_tabletId)}&station={Uri.EscapeDataString(selected.Code)}");
        _navigating = false;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Scan");

    private void SetFilter(string filter, Button active, Button other1, Button other2)
    {
        _filter = filter;
        var resources = Application.Current!.Resources;
        active.Style = (Style)resources["OutlineButtonActive"];
        other1.Style = (Style)resources["OutlineButton"];
        other2.Style = (Style)resources["OutlineButton"];
        RefreshAll();
    }

    private void OnFilterAllClicked(object sender, EventArgs e)
        => SetFilter("All", FilterAllButton, FilterAssignedButton, FilterFreeButton);

    private void OnFilterAssignedClicked(object sender, EventArgs e)
        => SetFilter("Assigned", FilterAssignedButton, FilterAllButton, FilterFreeButton);

    private void OnFilterFreeClicked(object sender, EventArgs e)
        => SetFilter("Free", FilterFreeButton, FilterAllButton, FilterAssignedButton);

    private static string DisplayTabletId(string id) => id.All(char.IsDigit) ? $"TABLET {id}" : id;
}