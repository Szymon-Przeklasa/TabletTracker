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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!IsScannerSupported)
            return;

        var granted = await TryRequestCameraPermissionAsync();
        if (granted)
        {
            CameraBarcodeReader.IsVisible = true;
            UnsupportedPanel.IsVisible = false;
            CameraBarcodeReader.IsDetecting = true;
        }
        else
        {
            ShowUnsupported("Brak dostępu do kamery.");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (IsScannerSupported)
            CameraBarcodeReader.IsDetecting = false;
    }

    private async Task<bool> TryRequestCameraPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
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