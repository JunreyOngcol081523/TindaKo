using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.ViewModel
{
    public partial class IncomingStocksLogsViewModel:ObservableObject
    {
        public List<string> Months { get; set; }
        [ObservableProperty] string selectedMonth;
        [ObservableProperty] int selectedYear;
        public List<int> Years { get; set; }
        //constructor
        public IncomingStocksLogsViewModel()
        {
            Months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
            .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            SelectedMonth = DateTime.Now.ToString("MMMM", CultureInfo.CurrentCulture);
            //populate the list of years
            Years = Enumerable.Range(DateTime.Now.Year - 10, 11).ToList();
            SelectedYear = DateTime.Now.Year;
        }
        private bool _isFilterEnabled;

        public bool IsFilterEnabled
        {
            get => _isFilterEnabled;
            set
            {
                _isFilterEnabled = value;
                OnPropertyChanged();
            }
        }

    }
}
