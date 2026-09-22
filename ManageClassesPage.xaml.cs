using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using TabletTracker.Models;
using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class ManageClassesPage : ContentPage
{
    private readonly DataStore _store = DataStore.Instance;

    public ObservableCollection<StudentItem> Students { get; } = new();

    private int _selectedClassIndex = -1;

    public ManageClassesPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshClasses();
    }

    private void RefreshClasses()
    {
        var classes = _store.Data.Classes;

        ClassPicker.ItemsSource = null;
        if (classes.Count == 0)
        {
            ClassPicker.ItemsSource = classes;
            _selectedClassIndex = -1;
            RemoveClassButton.IsVisible = false;
            StudentsCountLabel.Text = "0 uczniów";
            Students.Clear();
            return;
        }

        ClassPicker.ItemsSource = classes;
        ClassPicker.ItemDisplayBinding = new Binding(nameof(StudentClass.Name));

        if (_selectedClassIndex < 0 || _selectedClassIndex >= classes.Count)
            _selectedClassIndex = 0;
        ClassPicker.SelectedIndex = _selectedClassIndex;

        RemoveClassButton.IsVisible = true;
        RefreshStudents();
    }

    private void OnClassChanged(object sender, EventArgs e)
    {
        _selectedClassIndex = ClassPicker.SelectedIndex;
        RefreshStudents();
    }

    private void RefreshStudents()
    {
        var selectedClass = ClassPicker.SelectedItem as StudentClass;
        Students.Clear();

        if (selectedClass is null)
        {
            StudentsCountLabel.Text = "0 uczniów";
            RemoveStudentButton.IsVisible = false;
            return;
        }

        foreach (var student in selectedClass.Students)
            Students.Add(new StudentItem(student) { ClassName = selectedClass.Name });

        StudentsCountLabel.Text = selectedClass.Students.Count == 1
            ? "1 uczeń"
            : $"{selectedClass.Students.Count} uczniów";
        RemoveStudentButton.IsVisible = false;
    }

    private async void OnAddClassClicked(object sender, EventArgs e)
    {
        var name = ClassNameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (_store.Data.Classes.Any(c =>
                string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            await DisplayAlert("Klasa już istnieje", $"Klasa „{name}” już jest w systemie.", "OK");
            return;
        }

        var newClass = new StudentClass { Name = name };
        _store.Data.Classes.Add(newClass);
        _store.Save();

        ClassNameEntry.Text = "";
        _selectedClassIndex = _store.Data.Classes.Count - 1;
        RefreshClasses();
    }

    private async void OnRemoveClassClicked(object sender, EventArgs e)
    {
        if (ClassPicker.SelectedItem is not StudentClass selected)
            return;

        var confirmed = await DisplayAlert("Usuń klasę",
            $"Usunąć klasę „{selected.Name}” wraz z {selected.Students.Count} uczniami?", "Usuń", "Anuluj");
        if (!confirmed)
            return;

        _store.Data.Classes.Remove(selected);
        _store.Save();

        _selectedClassIndex = -1;
        RefreshClasses();
    }

    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        var selectedClass = ClassPicker.SelectedItem as StudentClass;
        if (selectedClass is null)
        {
            await DisplayAlert("Brak klasy", "Najpierw wybierz klasę, do której chcesz dodać ucznia.", "OK");
            return;
        }

        var name = StudentNameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        selectedClass.Students.Add(new Student { FullName = name });
        _store.Save();

        StudentNameEntry.Text = "";
        RefreshStudents();
    }

    private async void OnRemoveStudentClicked(object sender, EventArgs e)
    {
        var selectedClass = ClassPicker.SelectedItem as StudentClass;
        if (selectedClass is null || StudentsList.SelectedItem is not StudentItem selected)
            return;

        var confirmed = await DisplayAlert("Usuń ucznia",
            $"Usunąć ucznia „{selected.FullName}” z klasy „{selectedClass.Name}”?", "Usuń", "Anuluj");
        if (!confirmed)
            return;

        selectedClass.Students.RemoveAll(s => s.Id == selected.Id);
        _store.Save();

        StudentsList.SelectedItem = null;
        RefreshStudents();
    }

    private void OnStudentSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as StudentItem;
        foreach (var item in Students)
            item.IsSelected = ReferenceEquals(item, selected);

        RemoveStudentButton.IsVisible = selected is not null;
    }
}