using Microsoft.Maui.Controls;
using TabletTracker.Models;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class AssignClassPage : ContentPage, IQueryAttributable
{
    private readonly DataStore _store = DataStore.Instance;

    private string _tabletId = "";
    private Station? _station;
    private string? _selectedStudentId;

    public AssignClassPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Dispatcher.Dispatch(RefreshStudentsHeight);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        RefreshStudentsHeight();
    }

    private void RefreshStudentsHeight()
    {
        if (StudentsList is null || Height <= 0)
            return;
        var container = StudentsList.Parent as VisualElement;
        if (container is null || container.Y <= 0 || StudentsList.Y <= 0)
            return;
        StudentsList.HeightRequest = Math.Max(120, Height - container.Y - StudentsList.Y - 20);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("code", out var code) || code is not string codeStr || string.IsNullOrWhiteSpace(codeStr))
            return;

        _tabletId = DataStore.NormalizeCode(codeStr);
        TabletIdLabel.Text = DisplayTabletId(_tabletId);

        if (query.TryGetValue("station", out var station) && station is string stationStr)
        {
            _station = _store.FindStation(stationStr);
            StationLabel.Text = DisplayStationCode(_station?.Code ?? stationStr);
        }

        LoadClasses();
        RefreshUnassignButton();
    }

    private void LoadClasses()
    {
        var classes = _store.Data.Classes.ToList();
        if (classes.Count == 0)
        {
            ClassPicker.ItemsSource = null;
            NoClassesHint.IsVisible = true;
            StudentsList.ItemsSource = null;
            NoClassHint.IsVisible = false;
            RefreshAssignButton();
            return;
        }

        ClassPicker.ItemsSource = classes;
        ClassPicker.ItemDisplayBinding = new Binding(nameof(StudentClass.Name));
        ClassPicker.SelectedIndex = 0;
        NoClassesHint.IsVisible = false;
        RefreshStudents();
    }

    private void OnClassChanged(object sender, EventArgs e) => RefreshStudents();

    private void RefreshStudents()
    {
        var selectedClass = ClassPicker.SelectedItem as StudentClass;
        _selectedStudentId = null;

        if (selectedClass is null)
        {
            StudentsList.ItemsSource = null;
            StudentsList.IsVisible = false;
            NoClassHint.IsVisible = true;
            RefreshAssignButton();
            return;
        }

        var items = selectedClass.Students
            .Select(s => new StudentItem(s))
            .ToList();
        StudentsList.ItemsSource = items;
        StudentsList.IsVisible = items.Count > 0;
        NoClassHint.IsVisible = items.Count == 0;
        RefreshAssignButton();
    }

    private void OnStudentSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as StudentItem;
        foreach (var item in StudentsList.ItemsSource.Cast<StudentItem>())
            item.IsSelected = ReferenceEquals(item, selected);

        _selectedStudentId = selected?.Id;
        RefreshAssignButton();
    }

    private void RefreshAssignButton()
        => AssignButton.IsEnabled = ClassPicker.SelectedItem is not null && _selectedStudentId is not null;

    private void RefreshUnassignButton()
        => UnassignButton.IsVisible = _station?.StudentId is not null;

    private async void OnAssignClicked(object sender, EventArgs e)
    {
        if (ClassPicker.SelectedItem is not StudentClass selectedClass || _selectedStudentId is null)
            return;

        var student = _store.FindStudent(_selectedStudentId);
        if (student is null || _station is null || string.IsNullOrEmpty(_tabletId))
            return;

        // Tablet może zajmować tylko jedno stanowisko — zwolnij stare
        foreach (var other in _store.Data.Stations)
        {
            if (!ReferenceEquals(other, _station)
                && string.Equals(DataStore.NormalizeCode(other.TabletId), _tabletId,
                    StringComparison.OrdinalIgnoreCase))
            {
                other.TabletId = null;
                other.StudentId = null;
                other.AssignedAt = null;
            }
        }

        _station.TabletId = _tabletId;
        _station.StudentId = student.Id;
        _station.AssignedAt = DateTime.Now;

        _store.Data.Scans.Add(new ScanRecord
        {
            Timestamp = DateTime.Now,
            StationCode = _station.Code,
            TabletId = _tabletId,
            StudentName = student.FullName,
            ClassName = selectedClass.Name,
            TeacherName = _store.Data.Settings.TeacherName,
        });

        _store.Save();
        await Shell.Current.GoToAsync("//Scan");
    }

    private async void OnUnassignClicked(object sender, EventArgs e)
    {
        if (_station is null)
            return;

        _station.TabletId = null;
        _station.StudentId = null;
        _station.AssignedAt = null;
        _store.Save();
        RefreshUnassignButton();
        RefreshAssignButton();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private static string DisplayStationCode(string code) => code.All(char.IsDigit) ? $"STAN. {code}" : code;
    private static string DisplayTabletId(string id) => id.All(char.IsDigit) ? $"TABLET {id}" : id;
}