using Microsoft.Extensions.Logging;

namespace TabletTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("IBMPlexMono-Regular.ttf", "IBMPlexMonoRegular");
                    fonts.AddFont("IBMPlexMono-SemiBold.ttf", "IBMPlexMonoSemiBold");
                    fonts.AddFont("IBMPlexMono-Medium.ttf", "IBMPlexMonoMedium");
                    fonts.AddFont("IBMPlexMono-Bold.ttf", "IBMPlexMonoBold");
                    fonts.AddFont("IBMPlexSans-Regular.ttf", "IBMPlexSansRegular");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
