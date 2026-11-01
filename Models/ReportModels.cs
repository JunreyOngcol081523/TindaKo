namespace TindaKo.Models
{
    public class TrendPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    /// One row of the product ranking.
    public class ProductRankRow
    {
        public int Rank { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// All units sold in the period, including ones with unknown cost.
        public decimal QtySold { get; set; }

        /// Revenue and cost of sales with a KNOWN cost only, so profit is never inflated.
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }

        /// Units sold before cost tracking existed (UnitCost = 0). Not counted in Profit/Margin.
        public decimal UnknownCostQty { get; set; }

        public decimal Profit => Revenue - Cost;
        public decimal? MarginPercent => Revenue > 0 ? Math.Round(Profit / Revenue * 100, 1) : null;

        public string Subtitle
        {
            get
            {
                var margin = MarginPercent is { } m ? $"margin {m:0.#}%" : "no cost data";
                return $"{QtySold:0.##} sold  ·  {margin}";
            }
        }
    }

    public class DebtorRow
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    /// Everything the Reports page shows for one period, computed in one pass.
    public class ReportSnapshot
    {
        // Sales
        public int TransactionCount { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal CashSales { get; set; }
        public decimal CreditSales { get; set; }

        // Profit (known-cost sales only)
        public decimal KnownRevenue { get; set; }
        public decimal CostOfGoods { get; set; }
        public decimal UnknownCostSales { get; set; }
        public decimal Profit => KnownRevenue - CostOfGoods;
        public decimal? MarginPercent => KnownRevenue > 0 ? Math.Round(Profit / KnownRevenue * 100, 1) : null;

        // Money spent restocking in the period (cash flow, NOT subtracted from profit)
        public decimal StockPurchases { get; set; }

        public List<TrendPoint> Trend { get; set; } = new();
        public List<ProductRankRow> Products { get; set; } = new();

        // Current balances (not filtered by period)
        public decimal UnpaidTotal { get; set; }

        /// Utang payments actually received during the selected period (cash in).
        public decimal CollectedInPeriod { get; set; }
        public int OverdueCount { get; set; }
        public List<DebtorRow> TopDebtors { get; set; } = new();

        public decimal InventoryValue { get; set; }
        public int NoCostProductCount { get; set; }
    }
}
