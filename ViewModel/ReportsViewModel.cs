using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using TindaKo.Services;
namespace TindaKo.ViewModel
{
    public partial class ReportsViewModel : ObservableObject
    {
        private Chart _monthlySalesChart;
        private Chart _expensesSalesChart;
        public ObservableCollection<int> AvailableYears { get; } = new();
        public Chart MonthlySalesChart
        {
            get => _monthlySalesChart;
            set => SetProperty(ref _monthlySalesChart, value);
        }
        public Chart ExpensesSalesChart
        {
            get => _expensesSalesChart;
            set => SetProperty(ref _expensesSalesChart, value);
        }
        public ObservableCollection<string> SegmentOptions { get; }

        [ObservableProperty]
        private int selectedSegmentIndex = 0; // Default to first option
        [ObservableProperty]
        private bool isCustom;

        // Computed property to get the selected label
        public string? SelectedSegmentLabel => SegmentOptions.ElementAtOrDefault(SelectedSegmentIndex);

        // Whenever SelectedSegmentIndex changes, notify that SelectedSegmentLabel also changed
        partial void OnSelectedSegmentIndexChanged(int value)
        {
            OnPropertyChanged(nameof(SelectedSegmentLabel));
            IsCustom = SelectedSegmentLabel == "Custom";
        }

        [ObservableProperty]
        private string filterBy = "Monthly";
        public ReportsViewModel()
        {
            LoadAvailableYears();
            SelectedYear = DateTime.Now.Year; // Default to current year
            SegmentOptions = new ObservableCollection<string>
        {
            "Monthly", "Yearly", "Custom"
        };
            SelectedSegmentIndex = 0;
        }


        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    _= LoadMonthlySalesChart();
                }
            }
        }
        private async Task LoadMonthlySalesChart()
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

        private async Task LoadExpensesSales(string filterBy, DateTime dFrom, DateTime dTo)
        {
            var expenses = await IncomingStocksService.GetTotalAmountAsync(filterBy, dFrom, dTo);
            //var sales = await TransactionServices.GetTotalAmountAsync(filterBy, dFrom, dTo);
            //var monthlyexpenses = await IncomingStocksService.GetTotalIncomingStocksAsync();
            //await ToastHelper.ShowMessage($"Expenses: {expenses}, Sales: {sales}, Monthly Expenses: {monthlyexpenses}");

            var entries = await ChartHelper.GetExpensesSalesChartDataAsync(filterBy, dFrom, dTo);

            ExpensesSalesChart = new DonutChart
            {
                IsAnimated = true,
                AnimationDuration = TimeSpan.FromSeconds(1),
                Entries = entries ?? new List<ChartEntry>()
            };
        }
        [ObservableProperty]
        private DateTime dateFrom = DateTime.Today.AddDays(-7); // Default: 7 days ago

        [ObservableProperty]
        private DateTime dateTo = DateTime.Today; // Default: today

        public string SelectedDateRange => $"From {DateFrom:MMM dd, yyyy} to {DateTo:MMM dd, yyyy}";

        [RelayCommand]
        private async Task GenerateReportAsync()
        {

            if (DateFrom > DateTo)
            {
                await ToastHelper.ShowError("Start date cannot be later than end date.");
                return;
            }
            FilterBy = SelectedSegmentLabel ?? "Monthly"; 

            await LoadExpensesSales(FilterBy,DateFrom,DateTo);

        }

    }
}
