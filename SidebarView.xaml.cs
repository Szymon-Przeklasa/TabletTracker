using Microsoft.Maui.Controls;

namespace TabletTracker.Controls;

public partial class SidebarView : ContentView
{
    public SidebarView()
    {
        InitializeComponent();
    }

    // Nazwa aktywnej strony, ustawiana z code-behind każdej strony (np. "Dashboard")
    public static readonly BindableProperty ActiveRouteProperty =
        BindableProperty.Create(nameof(ActiveRoute), typeof(string), typeof(SidebarView), default(string),
            propertyChanged: OnActiveRouteChanged);

    public string ActiveRoute
    {
        get => (string)GetValue(ActiveRouteProperty);
        set => SetValue(ActiveRouteProperty, value);
    }

    private static void OnActiveRouteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SidebarView sidebar)
        {
            sidebar.RefreshActiveState((string)newValue);
        }
    }

    private void RefreshActiveState(string route)
    {
        // Reset wszystkich pozycji
        foreach (var (item, label) in AllItems())
        {
            item.SetDynamicResource(Border.StyleProperty, "SidebarItemFrame");
            item.Style = (Style)Application.Current!.Resources["SidebarItemFrame"];
            label.Style = (Style)Application.Current!.Resources["SidebarItemLabel"];
        }

        var (activeItem, activeLabel) = route switch
        {
            "Dashboard" => (MenuGlowneItem, MenuGlowneLabel),
            "Scan" => (SkanujItem, SkanujLabel),
            "Classes" => (KlasyItem, KlasyLabel),
            "Stations" => (StanowiskaItem, StanowiskaLabel),
            "History" => (HistoriaItem, HistoriaLabel),
            "Settings" => (UstawieniaItem, UstawieniaLabel),
            _ => (null, null)
        };

        if (activeItem is not null && activeLabel is not null)
        {
            activeItem.Style = (Style)Application.Current!.Resources["SidebarItemFrameActive"];
            activeLabel.Style = (Style)Application.Current!.Resources["SidebarItemLabelActive"];
        }
    }

    private IEnumerable<(Border, Label)> AllItems()
    {
        yield return (MenuGlowneItem, MenuGlowneLabel);
        yield return (SkanujItem, SkanujLabel);
        yield return (KlasyItem, KlasyLabel);
        yield return (StanowiskaItem, StanowiskaLabel);
        yield return (HistoriaItem, HistoriaLabel);
        yield return (UstawieniaItem, UstawieniaLabel);
    }

    private async void OnMenuGlowneTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Dashboard");
    private async void OnSkanujTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Scan");
    private async void OnKlasyTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Classes");
    private async void OnStanowiskaTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Stations");
    private async void OnHistoriaTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//History");
    private async void OnUstawieniaTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Settings");
}
