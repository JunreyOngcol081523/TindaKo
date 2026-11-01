using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;

using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class ReportsViewModel : ObservableObject
    {
        // =====================================================
        //  TABS
        // =====================================================
        public List<string> TabOptions { get; } = new() { "Overview", "Products", "Sold", "Utang" };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOverviewTab))]
        [NotifyPropertyChangedFor(nameof(IsProductsTab))]
        [NotifyPropertyChangedFor(nameof(IsSoldTab))]
        [NotifyPropertyChangedFor(nameof(IsUtangTab))]
        private int selectedTabIndex;

        public bool IsOverviewTab => SelectedTabIndex == 0;
        public bool IsProductsTab => SelectedTabIndex == 1;
        public bool IsSoldTab => SelectedTabIndex == 2;
        public bool IsUtangTab => SelectedTabIndex == 3;

        partial void OnSelectedTabIndexChanged(int value)
        {
            // the sold list is loaded lazily, only when its tab is opened and the period changed
            if (value == 2 && _soldDirty)
                _ = LoadSoldAsync();
        }

        // =====================================================
        //  PERIOD (one filter shared by every tab)
        // =====================================================
        public List<string> PeriodOptions { get; } = new() { "Today", "Week", "Month", "Year", "Custom" };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCustom))]
        private int selectedPeriodIndex = 2; // Month

        public bool IsCustom => SelectedPeriodIndex == 4;

        // only used when "Custom" is selected
        [ObservableProperty] private DateTime dateFrom = DateTime.Today.AddDays(-7);
        [ObservableProperty] private DateTime dateTo = DateTime.Today;

        [ObservableProperty] private string rangeText = string.Empty;

        partial void OnSelectedPeriodIndexChanged(int value) => _ = RefreshAsync();

        partial void OnDateFromChanged(DateTime value)
        {
            if (IsCustom) _ = RefreshAsync();
        }

        partial void OnDateToChanged(DateTime value)
        {
            if (IsCustom) _ = RefreshAsync();
        }

        private (DateTime Start, DateTime End) GetRange()
        {
            var today = DateTime.Today;

            return SelectedPeriodIndex switch
            {
                0 => (today, today),
                1 => (today.AddDays(-(((int)today.DayOfWeek + 6) % 7)), today), // Monday -> today
                2 => (new DateTime(today.Year, today.Month, 1), today),
                3 => (new DateTime(today.Year, 1, 1), today),
                _ => (DateFrom.Date, DateTo.Date)
            };
        }

        // =====================================================
        //  OVERVIEW
        // =====================================================
        [ObservableProperty] private bool isBusy;

        [ObservableProperty] private decimal salesTotal;
        [ObservableProperty] private int transactionCount;
        [ObservableProperty] private decimal cashSales;
        [ObservableProperty] private decimal creditSales;
        [ObservableProperty] private decimal profit;
        [ObservableProperty] private string marginText = string.Empty;
        [ObservableProperty] private decimal stockPurchases;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasProfitNote))]
        private string profitNote = string.Empty;

        public bool HasProfitNote => !string.IsNullOrEmpty(ProfitNote);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTrend))]
        [NotifyPropertyChangedFor(nameof(HasNoTrend))]
        private Chart? trendChart;

        public bool HasTrend => TrendChart is not null;
        public bool HasNoTrend => TrendChart is null;

        // =====================================================
        //  PRODUCT RANKING
        // =====================================================
        public List<string> RankSortOptions { get; } = new() { "Profit", "Units", "Margin" };

        [ObservableProperty] private int selectedRankSortIndex;

        partial void OnSelectedRankSortIndexChanged(int value) => ApplyRanking();

        [ObservableProperty] private ObservableCollection<ProductRankRow> rankedProducts = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasRankNote))]
        private string rankNote = string.Empty;

        public bool HasRankNote => !string.IsNullOrEmpty(RankNote);

        private List<ProductRankRow> _rankRows = new();

        private void ApplyRanking()
        {
            IEnumerable<ProductRankRow> sorted = SelectedRankSortIndex switch
            {
                1 => _rankRows.OrderByDescending(r => r.QtySold),
                2 => _rankRows
                        .OrderByDescending(r => r.MarginPercent.HasValue)
                        .ThenByDescending(r => r.MarginPercent ?? 0m),
                _ => _rankRows.OrderByDescending(r => r.Profit)
            };

            var list = sorted.ToList();
            for (var i = 0; i < list.Count; i++)
                list[i].Rank = i + 1;

            RankedProducts = new ObservableCollection<ProductRankRow>(list);

            var unknownQty = _rankRows.Sum(r => r.UnknownCostQty);
            RankNote = unknownQty > 0
                ? $"{unknownQty:0.##} units sold without cost data are not counted in Profit and Margin."
                : string.Empty;
        }

        // =====================================================
        //  SOLD ITEMS (itemized list, same data source as before)
        // =====================================================
        public ObservableCollection<OutgoingProductItem> SoldItems { get; } = new();

        [ObservableProperty] private decimal soldTotalAmount;
        [ObservableProperty] private decimal soldTotalQty;
        [ObservableProperty] private bool isSoldBusy;

        private bool _soldDirty = true;
        private int _soldVersion;

        private async Task LoadSoldAsync()
        {
            var (start, end) = GetRange();
            if (start > end) return;

            var version = ++_soldVersion;
            IsSoldBusy = true;
            try
            {
                var rows = (await SellServices.GetOutgoingProductsAsync(start, end)).ToList();
                if (version != _soldVersion) return; // a newer load superseded this one

                SoldItems.Clear();
                foreach (var r in rows)
                    SoldItems.Add(r);

                SoldTotalAmount = rows.Sum(r => r.TotalAmount);
                SoldTotalQty = rows.Sum(r => r.TotalQty);
                _soldDirty = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Reports] sold items failed: {ex}");
                await App.Navigator.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                if (version == _soldVersion) IsSoldBusy = false;
            }
        }

        // =====================================================
        //  UTANG + INVENTORY (current balances, not period-based)
        // =====================================================
        [ObservableProperty] private decimal unpaidTotal;
        [ObservableProperty] private decimal collectedInPeriod;
        [ObservableProperty] private int overdueCount;
        [ObservableProperty] private ObservableCollection<DebtorRow> topDebtors = new();
        [ObservableProperty] private decimal inventoryValue;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasInventoryNote))]
        private string inventoryNote = string.Empty;

        public bool HasInventoryNote => !string.IsNullOrEmpty(InventoryNote);

        // =====================================================
        //  REFRESH
        // =====================================================
        private int _version;

        /// Reloads every report for the selected period. Called when the page appears
        /// and whenever the period changes. Newer calls win over older, slower ones.
        public async Task RefreshAsync()
        {
            var (start, end) = GetRange();

            if (start > end)
            {
                await ToastHelper.ShowError("Start date cannot be later than end date.");
                return;
            }

            RangeText = start == end
                ? $"{start:MMM dd, yyyy}"
                : $"{start:MMM dd, yyyy} – {end:MMM dd, yyyy}";

            var version = ++_version;
            IsBusy = true;
            try
            {
                var snap = await ReportsService.BuildAsync(start, end);
                if (version != _version) return; // superseded by a newer refresh

                Apply(snap);

                _soldDirty = true;
                if (IsSoldTab)
                    _ = LoadSoldAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Reports] refresh failed: {ex}");
                await App.Navigator.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                if (version == _version) IsBusy = false;
            }
        }

        private void Apply(ReportSnapshot s)
        {
            SalesTotal = s.SalesTotal;
            TransactionCount = s.TransactionCount;
            CashSales = s.CashSales;
            CreditSales = s.CreditSales;
            StockPurchases = s.StockPurchases;

            Profit = s.Profit;
            MarginText = s.MarginPercent is { } m ? $"Margin {m:0.#}%" : "No cost data yet";
            ProfitNote = s.UnknownCostSales > 0
                ? $"₱{s.UnknownCostSales:N2} of sales has no cost data and is not counted in profit."
                : string.Empty;

            TrendChart = BuildTrendChart(s.Trend);

            _rankRows = s.Products;
            ApplyRanking();

            UnpaidTotal = s.UnpaidTotal;
            CollectedInPeriod = s.CollectedInPeriod;
            OverdueCount = s.OverdueCount;
            TopDebtors = new ObservableCollection<DebtorRow>(s.TopDebtors);

            InventoryValue = s.InventoryValue;
            InventoryNote = s.NoCostProductCount > 0
                ? $"{s.NoCostProductCount} product(s) in stock have no cost yet and are not valued."
                : string.Empty;
        }

        private static Chart? BuildTrendChart(List<TrendPoint> points)
        {
            if (points.Count == 0 || points.All(p => p.Amount <= 0))
                return null; // the page shows an empty message instead

            var max = points.Max(p => p.Amount);
            var dense = points.Count > 12;
            var color = SKColor.Parse("#16A085");

            // with many bars only the tallest one gets a value label, to keep the chart readable
            var entries = points.Select(p => new ChartEntry((float)p.Amount)
            {
                Label = p.Label,
                ValueLabel = p.Amount > 0 && (!dense || p.Amount == max)
                    ? p.Amount.ToString("N0", CultureInfo.InvariantCulture)
                    : string.Empty,
                Color = color
            }).ToList();

            return new BarChart
            {
                Entries = entries,
                IsAnimated = true,
                AnimationDuration = TimeSpan.FromSeconds(1),
                ValueLabelOrientation = Orientation.Horizontal,
                LabelOrientation = Orientation.Horizontal,
                LabelTextSize = dense ? 22 : 28
            };
        }
    }
}