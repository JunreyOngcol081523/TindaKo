using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
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
                .ConfigureSyncfusionToolkit()
                .ConfigureSyncfusionCore()
                .UseMicrocharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("centurygothic.ttf", "centurygothicRegular");
                    fonts.AddFont("centurygothic_bold.ttf", "centurygothicBold");
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
            builder.Services.AddSingleton<SellViewModel>();
            builder.Services.AddSingleton<SellPage>();
            builder.Services.AddSingleton<BuyerViewModel>();
            builder.Services.AddSingleton<BuyerPage>();
            builder.Services.AddSingleton<SalesViewModel>();
            builder.Services.AddSingleton<SalesPage>();
            builder.Services.AddSingleton<ProductAdjustViewModel>();
            builder.Services.AddSingleton<ProductStockAdjustPopup>();
            builder.Services.AddSingleton<SalesDetailsViewModel>();
            builder.Services.AddSingleton<SalesDetailsPage>();
            builder.Services.AddSingleton<DatabaseBackupPage>();
            builder.Services.AddSingleton<DBViewModel>();
            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<UserSettingsPage>();
            builder.Services.AddSingleton<ProductSettingsPage>();
            builder.Services.AddSingleton<UserSettingsViewModel>();
            builder.Services.AddSingleton<ProductSettingsViewModel>();
            builder.Services.AddSingleton<CreditorPage>();
            builder.Services.AddSingleton<CreditorViewModel>();
            builder.Services.AddSingleton<IncomingStocksPage>();
            builder.Services.AddSingleton<IncomingStocksViewModel>();
            builder.Services.AddSingleton<IncomingStocksPopup>();
            builder.Services.AddSingleton<IncomingStocksPopupViewModel>();
            builder.Services.AddSingleton<SellPageCartPopup>();
            builder.Services.AddSingleton<ReportsViewModel>();
            builder.Services.AddSingleton<ReportsPage>();
            builder.Services.AddSingleton<StoreSettings>();
            builder.Services.AddSingleton<StoreSettingsViewModel>();
        }
    }
}
