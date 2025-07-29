using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TindaKo.Helpers
{
    
    public static class CategoriesHelper
    {
        private static ObservableCollection<string> tempCategories;
        public static async Task<ObservableCollection<string>> LoadCategoriesFromJsonAsync()
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "categories.json");

            if (!File.Exists(filePath))
            {
                return new ObservableCollection<string>
                                {
                                    "Food and Snacks",
                                    "Beverages",
                                    "Cooking Essentials",
                                    "Personal Care",
                                    "Laundry and Cleaning",
                                    "Household Items",
                                    "School Supplies",
                                    "Mobile and Load",
                                    "Cigarettes and Alcohol",
                                    "Medicines",
                                    "Others"
                                };
            }
                

            var json = await File.ReadAllTextAsync(filePath);
            var categories = JsonSerializer.Deserialize<List<string>>(json);

            return new ObservableCollection<string>(categories ?? []);
        }

    }
}
