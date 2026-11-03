using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace TindaKo.ViewModel
{
    public partial class QuantityPopupViewModel : ObservableObject
    {
        private const int MaxDecimals = 3;
        private readonly Action<decimal> _onConfirm;

        public string ProductName { get; }
        public decimal Price { get; }
        public decimal StockLeft { get; }
        public string ConfirmText { get; }

        // set by the popup so this ViewModel can close it
        public Action? RequestClose { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtotal))]
        [NotifyPropertyChangedFor(nameof(CanConfirm))]
        [NotifyPropertyChangedFor(nameof(ErrorText))]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string quantityText = "1";

        public QuantityPopupViewModel(
            string productName, decimal price, decimal stockLeft,
            decimal initialQuantity, string confirmText, Action<decimal> onConfirm)
        {
            ProductName = productName;
            Price = price;
            StockLeft = stockLeft;
            ConfirmText = confirmText;
            _onConfirm = onConfirm;

            // stock may have dropped since the item was put in the cart
            quantityText = Format(Math.Min(Math.Max(initialQuantity, 0m), stockLeft));
        }

        // null while the text isn't a valid number (empty, ".", letters...)
        public decimal? Quantity => TryParse(QuantityText, out var q) ? q : null;

        public bool CanConfirm =>
            Quantity is decimal q && q > 0 && q <= StockLeft && Math.Round(q, MaxDecimals) == q;

        public decimal Subtotal => CanConfirm ? Quantity!.Value * Price : 0m;

        public bool HasError => ErrorText.Length > 0;

        public string ErrorText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(QuantityText)) return string.Empty;   // no scolding while typing
                if (Quantity is not decimal q) return "Maglagay ng tamang bilang.";
                if (q <= 0) return "Dapat mas mataas sa 0 ang dami.";
                if (Math.Round(q, MaxDecimals) != q) return $"Hanggang {MaxDecimals} decimal lang.";
                if (q > StockLeft) return $"Only {Format(StockLeft)} left in stock.";
                return string.Empty;
            }
        }

        [RelayCommand]
        private void Increase()
        {
            var current = Quantity ?? 0m;
            QuantityText = Format(Math.Min(current + 1, StockLeft));
        }

        [RelayCommand]
        private void Decrease()
        {
            var next = (Quantity ?? 0m) - 1;
            if (next <= 0) return;                   // never step down to zero or below
            QuantityText = Format(next);
        }

        [RelayCommand]
        private void Confirm()
        {
            if (!CanConfirm) return;

            _onConfirm(Normalize(Quantity!.Value));
            RequestClose?.Invoke();
        }

        private static bool TryParse(string? text, out decimal value)
        {
            var t = (text ?? string.Empty).Trim().Replace(',', '.');
            return decimal.TryParse(t, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
        }

        private static string Format(decimal value) =>
            value.ToString("0.###", CultureInfo.InvariantCulture);

        // 2.50 -> 2.5, so the cart never shows "2.50" or "2.500"
        private static decimal Normalize(decimal value) => value / 1.0000000000000000000000000000m;
    }
}