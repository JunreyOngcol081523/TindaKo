using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Helpers
{
    public class DialogHelper
    {
        // Helper methods for dialogs
        public static async Task ShowAlertAsync(string title, string message)
        {
            var page = GetCurrentPage();
            if (page != null)
            {
                await page.DisplayAlert(title, message, "OK");
            }
        }

        public static async Task<bool> ShowConfirmAsync(string title, string message)
        {
            var page = GetCurrentPage();
            if (page != null)
            {
                return await page.DisplayAlert(title, message, "Yes", "No");
            }
            return false;
        }

        private static Page GetCurrentPage()
        {
            var navigation = Application.Current?.MainPage?.Navigation;
            if (navigation?.NavigationStack?.Count > 0)
            {
                return navigation.NavigationStack[navigation.NavigationStack.Count - 1];
            }
            return Application.Current?.MainPage as Page;
        }
    }
}
