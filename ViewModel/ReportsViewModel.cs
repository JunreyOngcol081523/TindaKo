using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
namespace TindaKo.ViewModel
{
    public partial class ReportsViewModel : ObservableObject
    {
        private Chart _monthlySalesChart;
        public ObservableCollection<int> AvailableYears { get; } = new();
        public Chart MonthlySalesChart
        {
            get => _monthlySalesChart;
            set => SetProperty(ref _monthlySalesChart, value);
        }
        public ReportsViewModel()
        {
            LoadMonthlySalesChart();
            LoadAvailableYears();
            SelectedYear = DateTime.Now.Year; // Default to current year
        }


        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    LoadMonthlySalesChart();
                }
            }
        }
        private async void LoadMonthlySalesChart()
        {
            try
            {

                var entries = await ChartHelper.MonthlySalesEntry(SelectedYear);
                if (entries != null && entries.Any())
                {
                    MonthlySalesChart = new BarChart()
                    {
                        //LabelTextSize = 30,
                        IsAnimated = true,
                        AnimationDuration = TimeSpan.FromSeconds(1),
                        //BackgroundColor = SKColor.Parse("#f0f0f0"),
                        ValueLabelOrientation = Orientation.Horizontal,
                        Entries = entries ?? new List<ChartEntry>()
                    };
                }
                else
                {
                    MonthlySalesChart = null; // or show an "empty" message UI
                }

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void LoadAvailableYears()
        {
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year >= currentYear - 5; year--)
            {
                AvailableYears.Add(year);
            }
        }
    }
}
