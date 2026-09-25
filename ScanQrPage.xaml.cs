using ZXing.Net.Maui;

namespace TabletTracker.Pages;

public partial class ScanQrPage : ContentPage
{
    private static readonly bool IsScannerSupported =
        DeviceInfo.Platform == DevicePlatform.Android ||
        DeviceInfo.Platform == DevicePlatform.iOS ||
        DeviceInfo.Platform == DevicePlatform.MacCatalyst;

    private bool _navigating;

    public ScanQrPage()
    {
        InitializeComponent();

        CameraBarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            TryHarder = true,
            Multiple = false,
        };

        if (!IsScannerSupported)
            ShowUnsupported("Skanowanie kodów QR wymaga urządzenia z kamerą.");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!IsScannerSupported)
            return;

        RefreshScannerState();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (IsScannerSupported)
            CameraBarcodeReader.IsDetecting = false;
    }

    // Uprawnienie pytamy na starcie aplikacji — tutaj tylko sprawdzamy jego stan.
    private async void RefreshScannerState()
    {
        if (await IsCameraGrantedAsync())
        {
            CameraBarcodeReader.IsVisible = true;
            UnsupportedPanel.IsVisible = false;
            EnableScanning();
        }
        else
        {
            ShowUnsupported("Brak dostępu do kamery.");
        }
    }

    // Aparat uruchamia się dopiero, gdy handler kontrolki jest dołączony.
    // Przy pierwszym uruchomieniu handler może jeszcze nie istnieć
    // — ponawiamy aż będzie gotowy.
    private void EnableScanning()
    {
        if (CameraBarcodeReader.Handler is not null)
        {
            CameraBarcodeReader.IsDetecting = true;
            return;
        }
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), EnableScanning);
    }

    private static async Task<bool> IsCameraGrantedAsync()
    {
        try
        {
            return await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var result = e.Results?.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.Value));
        if (result is null)
            return;

        Dispatcher.Dispatch(async () =>
        {
            if (_navigating)
                return;

            _navigating = true;
            CameraBarcodeReader.IsDetecting = false;
            AssignmentFlow.StationCode = null;
            AssignmentFlow.TabletId = result.Value;
            await Shell.Current.GoToAsync("//AssignStation");
            _navigating = false;
        });
    }

    private void ShowUnsupported(string message)
    {
        CameraBarcodeReader.IsVisible = false;
        UnsupportedMessageLabel.Text = message;
        UnsupportedPanel.IsVisible = true;
    }
}