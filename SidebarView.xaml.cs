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
        var resources = Application.Current!.Resources;

        foreach (var (item, label) in AllItems())
        {
            item.Style = (Style)resources["SidebarItemFrame"];
            label.Style = (Style)resources["SidebarItemLabel"];
        }

        var (activeItem, activeLabel) = route switch
        {
            "Dashboard" => ((Border?)MenuGlowneItem, (Label?)MenuGlowneLabel),
            "Scan" => ((Border?)SkanujItem, (Label?)SkanujLabel),
            "Classes" => ((Border?)KlasyItem, (Label?)KlasyLabel),
            "Stations" => ((Border?)StanowiskaItem, (Label?)StanowiskaLabel),
            "History" => ((Border?)HistoriaItem, (Label?)HistoriaLabel),
            "Settings" => ((Border?)UstawieniaItem, (Label?)UstawieniaLabel),
            _ => (null, null)
        };

        if (activeItem is not null && activeLabel is not null)
        {
            activeItem.Style = (Style)resources["SidebarItemFrameActive"];
            activeLabel.Style = (Style)resources["SidebarItemLabelActive"];
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

    private static async Task NavigateToAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch
        {
            // Nawigacja absolutna czyści stos docelowej zakładki; błędy ignorujemy,
            // żeby wyjątek w async void nie wywrócił aplikacji.
        }
    }

    private async void OnMenuGlowneTapped(object sender, EventArgs e) => await NavigateToAsync("//Dashboard");
    private async void OnSkanujTapped(object sender, EventArgs e) => await NavigateToAsync("//Scan");
    private async void OnKlasyTapped(object sender, EventArgs e) => await NavigateToAsync("//Classes");
    private async void OnStanowiskaTapped(object sender, EventArgs e) => await NavigateToAsync("//Stations");
    private async void OnHistoriaTapped(object sender, EventArgs e) => await NavigateToAsync("//History");
    private async void OnUstawieniaTapped(object sender, EventArgs e) => await NavigateToAsync("//Settings");
}
