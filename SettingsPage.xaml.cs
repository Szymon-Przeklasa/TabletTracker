using TabletTracker.Services;

namespace TabletTracker.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly DataStore _store = DataStore.Instance;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TeacherNameEntry.Text = _store.Data.Settings.TeacherName;
        SavedHint.IsVisible = false;
    }

    private void OnSaveTeacherClicked(object sender, EventArgs e)
    {
        _store.Data.Settings.TeacherName = TeacherNameEntry.Text?.Trim() ?? "";
        _store.Save();
        SavedHint.IsVisible = true;
    }
}