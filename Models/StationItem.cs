using System.ComponentModel;

namespace TabletTracker.Models;

public class StationItem : INotifyPropertyChanged
{
    public string Code { get; }

    public string DisplayCode => DataFormat.Station(Code);

    private string _tabletInfo = "";
    public string TabletInfo
    {
        get => _tabletInfo;
        set { _tabletInfo = value; OnPropertyChanged(nameof(TabletInfo)); }
    }

    private string _studentInfo = "Wolne";
    public string StudentInfo
    {
        get => _studentInfo;
        set { _studentInfo = value; OnPropertyChanged(nameof(StudentInfo)); }
    }

    private bool _isAssigned;
    public bool IsAssigned
    {
        get => _isAssigned;
        set { _isAssigned = value; OnPropertyChanged(nameof(IsAssigned)); }
    }

    public StationItem(string code)
    {
        Code = code;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}