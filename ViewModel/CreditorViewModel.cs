using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class CreditorViewModel : ObservableObject
    {
        public ObservableCollection<string> SegmentOptions { get; } = new() { "All", "Past Due", "Paid", "Unpaid" };

        [ObservableProperty] private ObservableCollection<Creditor> filteredCreditors = new();
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private int selectedSegmentIndex;
        [ObservableProperty] private decimal totalAmount;
        [ObservableProperty] private string totalLabel = "Balance due";
        [ObservableProperty] private bool isBusy;

        private List<Creditor> _all = new();
        private readonly Dictionary<int, CancellationTokenSource> _noteSaves = new();

        // Search and filter work on the list already in memory: no database call per keystroke.
        partial void OnSelectedSegmentIndexChanged(int value) => ApplyFilter();
        partial void OnSearchTextChanged(string value) => ApplyFilter();

        // ───────────────────────── Load + filter ─────────────────────────

        [RelayCommand]
        public async Task LoadCreditorsAsync()
        {
            IsBusy = true;
            try
            {
                var fresh = await CreditorServices.GetCreditorsAsync();

                // keep expanded sections open across reloads
                var previous = _all.ToDictionary(c => c.Id);

                foreach (var c in fresh)
                {
                    if (previous.TryGetValue(c.Id, out var old))
                    {
                        c.IsNotesExpanded = old.IsNotesExpanded;
                        c.IsHistoryExpanded = old.IsHistoryExpanded;
                    }

                    c.NotesChanged += OnNotesChanged;
                }

                _all = fresh;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Creditors] load failed: {ex}");
                await ToastHelper.ShowError($"Failed to load utang: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilter()
        {
            var today = DateTime.Today;
            IEnumerable<Creditor> query = _all;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.Trim();
                query = query.Where(c => c.CreditorName.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            query = SelectedSegmentIndex switch
            {
                1 => query.Where(c => !c.IsPaid && c.DueDate.Date < today),
                2 => query.Where(c => c.IsPaid),
                3 => query.Where(c => !c.IsPaid),
                _ => query
            };

            // unpaid first, soonest due date first
            var list = query.OrderBy(c => c.IsPaid).ThenBy(c => c.DueDate).ToList();

            FilteredCreditors = new ObservableCollection<Creditor>(list);

            var settledView = SelectedSegmentIndex == 2;
            TotalLabel = settledView ? "Total settled" : "Balance due";
            TotalAmount = settledView ? list.Sum(c => c.CreditAmount) : list.Sum(c => c.Balance);
        }

        // ───────────────────────── Payments ─────────────────────────

        [RelayCommand]
        private void TogglePayPanel(Creditor? creditor)
        {
            if (creditor is null || creditor.IsPaid) return;

            creditor.IsPayPanelOpen = !creditor.IsPayPanelOpen;

            if (creditor.IsPayPanelOpen)
            {
                // pre-filled with the full balance: change it for a partial payment
                creditor.PayAmountText = creditor.Balance.ToString("0.##", CultureInfo.CurrentCulture);
                creditor.PayNote = string.Empty;
            }
        }

        [RelayCommand]
        private async Task SavePaymentAsync(Creditor? creditor)
        {
            if (creditor is null || IsBusy) return;

            if (!TryParseAmount(creditor.PayAmountText, out var amount) || amount <= 0)
            {
                await ToastHelper.ShowError("Enter a valid payment amount.");
                return;
            }

            amount = Math.Round(amount, 2);

            if (amount > creditor.Balance)
            {
                await ToastHelper.ShowError($"Payment is more than the balance (₱{creditor.Balance:N2}).");
                return;
            }

            var balanceAfter = creditor.Balance - amount;

            bool confirm = await DialogHelper.ShowConfirmAsync(
                "Receive Payment",
                $"Receive ₱{amount:N2} from {creditor.CreditorName}?\nBalance after: ₱{balanceAfter:N2}");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await CreditorServices.AddPaymentAsync(creditor.Id, amount, creditor.PayNote);
                await LoadCreditorsAsync();
                await ToastHelper.ShowMessage(balanceAfter <= 0 ? "Fully paid!" : "Payment saved.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Creditors] payment failed: {ex}");
                await ToastHelper.ShowError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task VoidPaymentAsync(CreditPayment? payment)
        {
            if (payment is null || payment.IsVoided || IsBusy) return;

            bool confirm = await DialogHelper.ShowConfirmAsync(
                "Void Payment",
                $"Void the ₱{payment.Amount:N2} payment from {payment.DatePaid:MMM dd, yyyy}?\nThe balance will go back up.");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await CreditorServices.VoidPaymentAsync(payment.PaymentId);
                await LoadCreditorsAsync();
                await ToastHelper.ShowMessage("Payment voided.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Creditors] void failed: {ex}");
                await ToastHelper.ShowError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleHistory(Creditor? creditor)
        {
            if (creditor is null) return;
            creditor.IsHistoryExpanded = !creditor.IsHistoryExpanded;
        }

        private static bool TryParseAmount(string? text, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var t = text.Trim();
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
                || decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        // ───────────────────────── Notes ─────────────────────────

        [RelayCommand]
        private void ToggleNotes(Creditor? creditor)
        {
            if (creditor is null) return;
            creditor.IsNotesExpanded = !creditor.IsNotesExpanded;
        }

        [RelayCommand]
        private async Task ClearNotesAsync(Creditor? creditor)
        {
            if (creditor is null) return;

            bool confirm = await DialogHelper.ShowConfirmAsync(
                "Clear Notes",
                "Are you sure you want to clear the notes?");

            if (!confirm) return;

            try
            {
                creditor.Notes = string.Empty; // the debounced save below writes it too
                await CreditorServices.UpdateNotesAsync(creditor.Id, string.Empty);
                await ToastHelper.ShowMessage("Notes cleared.");
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError($"Failed to clear notes: {ex.Message}");
            }
        }

        // Debounced autosave: waits until typing pauses, then saves ONLY the Notes column.
        private void OnNotesChanged(Creditor creditor)
        {
            if (_noteSaves.Remove(creditor.Id, out var previous))
            {
                previous.Cancel();
                previous.Dispose();
            }

            var cts = new CancellationTokenSource();
            _noteSaves[creditor.Id] = cts;
            _ = SaveNotesLaterAsync(creditor, cts.Token);
        }

        private static async Task SaveNotesLaterAsync(Creditor creditor, CancellationToken token)
        {
            try
            {
                await Task.Delay(800, token);
                await CreditorServices.UpdateNotesAsync(creditor.Id, creditor.Notes);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Creditors] notes autosave failed: {ex}");
            }
        }
    }
}