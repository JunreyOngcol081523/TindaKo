using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Maui.Devices;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
namespace TindaKo.Helpers
{

    public static class ToastHelper
    {
        public static async Task ShowMessage(string message, string title = null)
        {
#if ANDROID || IOS || MACCATALYST
            var toast = Toast.Make(message, ToastDuration.Short, 14);
            await toast.Show();
#else
        
        var page = DialogHelper.GetCurrentPage();
        if (page != null)
            await page.DisplayAlert(title ?? "Info", message, "OK");
#endif
        }

        public static async Task ShowError(string message)
        {
#if ANDROID || IOS || MACCATALYST
            var toast = Toast.Make(message, ToastDuration.Long, 14);
            await toast.Show();
#else
        var page = DialogHelper.GetCurrentPage();
        if (page != null)
            await page.DisplayAlert("Error", message, "OK");
#endif
        }
    }

}
