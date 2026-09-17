using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TabletTracker.Pages;

public partial class ManageClassesPage : ContentPage
{
    public ObservableCollection<StudentItem> Students { get; } = new()
    {
        new StudentItem("Javonster Javowy", "JJ"),
        new StudentItem("Fonk Skibididonk", "FS") { IsSelected = true },
        new StudentItem("King Javon", "KJ"),
        new StudentItem("Jozef Javistar", "JJ"),
        new StudentItem("Jafonk Yarayara", "JY"),
        new StudentItem("Kamil Kovalenko", "KK"),
    };

    private StudentItem? _selectedStudent;
    public StudentItem? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            // Odznacz poprzedniego, zaznacz nowego — imitacja zachowania z makiety
            foreach (var s in Students)
                s.IsSelected = ReferenceEquals(s, value);
            _selectedStudent = value;
        }
    }

    public ManageClassesPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}

public class StudentItem : INotifyPropertyChanged
{
    public string FullName { get; }
    public string Initials { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    public StudentItem(string fullName, string initials)
    {
        FullName = fullName;
        Initials = initials;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
