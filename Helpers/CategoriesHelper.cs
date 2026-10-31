using System.Collections.ObjectModel;
using TindaKo.Services;

namespace TindaKo.Helpers
{
    
    public static class CategoriesHelper
    {
        public static async Task<ObservableCollection<string>> LoadCategoriesFromJsonAsync()
        {
            return await CreateDefaultCategories();
        }

        private static async Task<ObservableCollection<string>> CreateDefaultCategories()
        {
            var categories = await ProductService.GetCategoriesAsync();
            return new ObservableCollection<string>(categories);
        }
    }
}
