using System.ComponentModel;

namespace TabletTracker.Models;

public class StudentItem : INotifyPropertyChanged
{
    public string Id { get; }

    private string _fullName;
    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(nameof(FullName)); OnPropertyChanged(nameof(Initials)); }
    }

    public string FullNameWithClass => string.IsNullOrWhiteSpace(ClassName) ? FullName : $"{FullName} · {ClassName}";

    public string ClassName { get; set; } = "";

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    public StudentItem(Student student) : this(student.Id, student.FullName)
    {
    }

    public StudentItem(string id, string fullName)
    {
        Id = id;
        _fullName = fullName;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}