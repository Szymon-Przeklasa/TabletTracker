using Microsoft.Maui.Controls;
using TabletTracker.Models;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class AssignClassPage : ContentPage
{
    private readonly DataStore _store = DataStore.Instance;

    private string _tabletId = "";
    private Station? _station;
    private string? _selectedStudentId;
    private bool _busy;

    public AssignClassPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _tabletId = DataStore.NormalizeCode(AssignmentFlow.TabletId);
        TabletIdLabel.Text = DataFormat.Tablet(_tabletId);

        _station = AssignmentFlow.StationCode is { } code ? _store.FindStation(code) : null;
        StationLabel.Text = DataFormat.Station(_station?.Code ?? "—");

        LoadClasses();
        RefreshUnassignButton();

        Dispatcher.Dispatch(RefreshStudentsHeight);
    }

    private void OnStudentsListSizeChanged(object? sender, EventArgs e) => RefreshStudentsHeight();

    private void RefreshStudentsHeight()
    {
        if (StudentsArea is null || StudentsList is null || StudentsArea.Height <= 0 || StudentsList.Y <= 0)
            return;
        var target = Math.Max(120, StudentsArea.Height - StudentsList.Y);
        if (Math.Abs(StudentsList.HeightRequest - target) > 1)
            StudentsList.HeightRequest = target;
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
        if (StudentsList.ItemsSource is IEnumerable<StudentItem> items)
        {
            foreach (var item in items)
                item.IsSelected = ReferenceEquals(item, selected);
        }

        _selectedStudentId = selected?.Id;
        RefreshAssignButton();
    }

    private void RefreshAssignButton()
        => AssignButton.IsEnabled = ClassPicker.SelectedItem is not null && _selectedStudentId is not null;

    private void RefreshUnassignButton()
        => UnassignButton.IsVisible = _station?.StudentId is not null;

    private async void OnAssignClicked(object sender, EventArgs e)
    {
        if (_busy)
            return;

        if (ClassPicker.SelectedItem is not StudentClass selectedClass || _selectedStudentId is null)
            return;

        var student = _store.FindStudent(_selectedStudentId);
        if (student is null || _station is null || string.IsNullOrEmpty(_tabletId))
            return;

        _busy = true;

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

        AssignmentFlow.Reset();

        // Wróć do skanowania (nawigacja absolutna czyści stos przypisania)
        try
        {
            await Shell.Current.GoToAsync("//Scan");
        }
        catch
        {
            _busy = false;
        }
    }

    private async void OnUnassignClicked(object sender, EventArgs e)
    {
        if (_busy || _station is null)
            return;

        _station.TabletId = null;
        _station.StudentId = null;
        _station.AssignedAt = null;
        _store.Save();
        RefreshUnassignButton();
        RefreshAssignButton();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_busy)
            return;

        _busy = true;
        try
        {
            await Shell.Current.GoToAsync("//AssignStation"); // z powrotem do wyboru stanowiska
        }
        catch
        {
            // ignore
        }
        _busy = false;
    }
}