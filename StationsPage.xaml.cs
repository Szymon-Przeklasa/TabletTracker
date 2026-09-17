using System.Collections.ObjectModel;

namespace TabletTracker.Pages;

public partial class StationsPage : ContentPage
{
    public ObservableCollection<Station> Stations { get; } = new()
    {
        new Station("01", "Javonster Javowy", true),
        new Station("02", "Fonk Skibididonk", true),
        new Station("03", "Wolne", false),
        new Station("04", "King Javon", true),
        new Station("05", "Jozef Javistar", true),
        new Station("06", "Wolne", false),
    };

    public StationsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}

public record Station(string Number, string StudentName, bool IsAssigned);
