namespace TabletTracker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await RequestCameraPermissionAsync();
        }

        // Uprawnienie do aparatu pytamy raz, przy starcie aplikacji,
        // żeby ekran skanowania nie blokował się dialogiem na starcie widoku.
        private static async Task RequestCameraPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                    await Permissions.RequestAsync<Permissions.Camera>();
            }
            catch
            {
                // Ekran skanowania pokaże komunikat o braku uprawnienia.
            }
        }
    }
}