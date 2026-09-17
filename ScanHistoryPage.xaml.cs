using System.Collections.ObjectModel;

namespace TabletTracker.Pages;

public partial class ScanHistoryPage : ContentPage
{
    public ObservableCollection<HistoryEntry> Entries { get; } = new()
    {
        new HistoryEntry("14.09", "10:18", "07", "Javonster Javowy", "5TP 1", "M. Javappen"),
        new HistoryEntry("14.09", "10:11", "03", "Fonk Skibididonk", "5TP 1", "M. Javappen"),
        new HistoryEntry("14.09", "09:58", "14", "King Javon", "5TP 1", "M. Javappen"),
        new HistoryEntry("13.09", "14:32", "11", "Samuel L. Javickson", "3TP 1", "L. Javilton"),
        new HistoryEntry("13.09", "14:20", "05", "Morgan Javiman", "3TP 1", "L. Javilton"),
        new HistoryEntry("12.09", "11:05", "09", "Kamil Kovalenko", "5TP 1", "M. Javappen"),
        new HistoryEntry("12.09", "10:47", "02", "Jozef Javistar", "5TP 1", "M. Javappen"),
    };

    public ScanHistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}

public record HistoryEntry(string Date, string Time, string Station, string Student, string Class, string Teacher);
