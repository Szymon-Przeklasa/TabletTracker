using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using TabletTracker.Models;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class StationsPage : ContentPage
{
    private readonly DataStore _store = DataStore.Instance;

    public ObservableCollection<StationItem> Stations { get; } = new();

    private string _filter = "All";

    public StationsPage()
    {
        InitializeComponent();
        BindingContext = this;
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

    private void OnStationsGridSizeChanged(object? sender, EventArgs e) => RefreshGridHeight();

    private void RefreshGridHeight()
    {
        if (StationsGrid is null || GridArea is null || GridArea.Height <= 0)
            return;
        var target = Math.Max(120, GridArea.Height);
        if (Math.Abs(StationsGrid.HeightRequest - target) > 1)
            StationsGrid.HeightRequest = target;
    }

    private void RefreshAll()
    {
        var data = _store.Data;

        var assigned = 0;
        Stations.Clear();

        foreach (var station in data.Stations)
        {
            var item = new StationItem(station.Code)
            {
                IsAssigned = station.StudentId is not null,
                TabletInfo = string.IsNullOrWhiteSpace(station.TabletId)
                    ? ""
                    : $"TABLET {station.TabletId}",
                StudentInfo = BuildStudentInfo(station),
            };
            if (item.IsAssigned)
                assigned++;

            var show = _filter == "All"
                || (_filter == "Assigned" && item.IsAssigned)
                || (_filter == "Free" && !item.IsAssigned);
            if (show)
                Stations.Add(item);
        }

        AssignedCountLabel.Text = assigned.ToString();
        FreeCountLabel.Text = (data.Stations.Count - assigned).ToString();
        TotalCountLabel.Text = data.Stations.Count.ToString();
        SubtitleLabel.Text = $"Sala P1 — {data.Stations.Count} stanowisk";
    }

    private string BuildStudentInfo(Station station)
    {
        if (station.StudentId is null)
            return "Wolne";

        var name = _store.StudentName(station.StudentId);
        var cls = _store.ClassNameOfStudent(station.StudentId);
        return string.IsNullOrWhiteSpace(cls) ? name! : $"{name} · {cls}";
    }

    private void OnAddStationClicked(object sender, EventArgs e)
    {
        var code = StationCodeEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return;

        if (_store.FindStation(code) is not null)
        {
            DisplayAlert("Stanowisko już istnieje", $"Stanowisko „{code}” już jest w systemie.", "OK");
            return;
        }

        _store.Data.Stations.Add(new Station { Code = code });
        _store.Save();
        StationCodeEntry.Text = "";
        RefreshAll();
    }

    private void OnUnassignStationClicked(object sender, EventArgs e)
    {
        if (StationsGrid.SelectedItem is not StationItem selected)
            return;

        var station = _store.FindStation(selected.Code);
        if (station is null)
            return;

        station.TabletId = null;
        station.StudentId = null;
        station.AssignedAt = null;
        _store.Save();
        StationsGrid.SelectedItem = null;
        RefreshAll();
    }

    private void OnUnassignAllClicked(object sender, EventArgs e)
    {
        foreach (var station in _store.Data.Stations)
        {
            station.TabletId = null;
            station.StudentId = null;
            station.AssignedAt = null;
        }
        _store.Save();
        StationsGrid.SelectedItem = null;
        RefreshAll();
    }

    private void OnRemoveStationClicked(object sender, EventArgs e)
    {
        if (StationsGrid.SelectedItem is not StationItem selected)
            return;

        var station = _store.FindStation(selected.Code);
        if (station is null)
            return;

        _store.Data.Stations.Remove(station);
        _store.Save();
        StationsGrid.SelectedItem = null;
        RefreshAll();
    }

    private void OnStationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as StationItem;
        RemoveStationButton.IsVisible = selected is not null;
        UnassignStationButton.IsVisible = selected?.IsAssigned == true;
    }

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
}