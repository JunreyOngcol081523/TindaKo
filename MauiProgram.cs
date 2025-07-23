using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TindaKo.ViewModel;
using TindaKo.Views;

namespace TindaKo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            IncludeViews(builder);
            return builder.Build();
        }
        private static void IncludeViews(MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<MenuViewModel>();
            builder.Services.AddSingleton<MenuPage>();
            builder.Services.AddSingleton<ProductViewModel>();
            builder.Services.AddSingleton<ProductPage>();
        }
    }
}
