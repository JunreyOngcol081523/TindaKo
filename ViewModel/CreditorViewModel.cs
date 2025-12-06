using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using System.Timers;

namespace TindaKo.ViewModel
{
    public partial class CreditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Creditor> filteredCreditors;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private bool isAllChecked = true;

        [ObservableProperty]
        private bool isPastDueChecked;

        [ObservableProperty]
        private bool isPaidChecked;

        [ObservableProperty]
        private bool isUnpaidChecked;
        [ObservableProperty]
        private decimal totalAmount;
        public ObservableCollection<string> SegmentOptions { get; }

        [ObservableProperty]
        private int selectedSegmentIndex;
        [ObservableProperty]
        private bool isBusy;
        public CreditorViewModel()
        {
            _ = LoadCreditorsAsync();
            SegmentOptions = new ObservableCollection<string>
        {
            "All", "Past Due", "Paid", "Unpaid"
        };
        }
        partial void OnSelectedSegmentIndexChanged(int value)
        {
            IsAllChecked = value == 0;
            IsPastDueChecked = value == 1;
            IsPaidChecked = value == 2;
            IsUnpaidChecked = value == 3;
        }
        partial void OnSearchTextChanged(string value)
        {
            _ = LoadCreditorsAsync();
        }

        partial void OnIsAllCheckedChanged(bool value)
        {
            if (value)
            {
                IsPastDueChecked = false;
                IsPaidChecked = false;
                IsUnpaidChecked = false;
                _ = LoadCreditorsAsync();
            }
        }

        partial void OnIsPastDueCheckedChanged(bool value)
        {
            if (value)
            {
                IsAllChecked = false;
                IsPaidChecked = false;
                IsUnpaidChecked = false;
                _ = LoadCreditorsAsync();
            }
        }

        partial void OnIsPaidCheckedChanged(bool value)
        {
            if (value)
            {
                IsAllChecked = false;
                IsPastDueChecked = false;
                IsUnpaidChecked = false;
                _ = LoadCreditorsAsync();
            }
        }

        partial void OnIsUnpaidCheckedChanged(bool value)
        {
            if (value)
            {
                IsAllChecked = false;
                IsPastDueChecked = false;
                IsPaidChecked = false;
                _ = LoadCreditorsAsync();
            }
        }

        [RelayCommand]
        private async Task LoadCreditorsAsync()
        {
            var allCreditors = await CreditorServices.GetCreditorsAsync();
            // Attach debounce handler for each creditor
            foreach (var c in allCreditors)
            {
                c.NotesChanged -= DebounceAutoSave;
                c.NotesChanged += DebounceAutoSave;
            }
            ApplyFilter(allCreditors);
        }

        private void ApplyFilter(List<Creditor> creditors)
        {
            var filtered = creditors.AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c => c.CreditorName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Radio button filters
            if (IsPastDueChecked)
            {
                filtered = filtered.Where(c => c.DueDate.Date < DateTime.Today && !c.IsPaid);
            }
            else if (IsPaidChecked)
            {
                filtered = filtered.Where(c => c.IsPaid);
            }
            else if (IsUnpaidChecked)
            {
                filtered = filtered.Where(c => !c.IsPaid);
            }
            var filteredList = filtered.OrderByDescending(c => c.DueDate).ToList();

            // Set collection
            FilteredCreditors = new ObservableCollection<Creditor>(filteredList);

            // Compute total
            TotalAmount = filteredList.Sum(c => c.CreditAmount);
        }

        [RelayCommand]
        private async Task MarkAsPaidAsync(Creditor creditor)
        {
            if (creditor == null || creditor.IsPaid)
                return;

            // Update the properties
            creditor.IsPaid = true;
            creditor.PaidDate = DateTime.Today;
            bool confirm = await DialogHelper.ShowConfirmAsync(
                "Mark as Paid",
                $"Mark {creditor.CreditorName}'s credit of ₱{creditor.CreditAmount:N2} as paid?");
            if (!confirm) return;


            // Reload the list with the updated values
            await LoadCreditorsAsync();

            try
            {

                // Save the update to the database
                await CreditorServices.UpdateCreditorAsync(creditor);
                await LoadCreditorsAsync();
                await DialogHelper.ShowAlertAsync("Success", "Credit marked as paid!");
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowAlertAsync("Error", $"Failed to mark as paid: {ex.Message}");
            }
            
        }
        [RelayCommand]
        public async Task MarkAsUnpaidAsync(Creditor creditor)
        {
            if (creditor == null || !creditor.IsPaid) return;
            creditor.IsPaid = false;
            creditor.PaidDate = null;
            bool confirm = await DialogHelper.ShowConfirmAsync(
                "Mark as Unpaid",
                $"Mark {creditor.CreditorName}'s credit as unpaid again?");

            if (!confirm) return;

            try
            {

                // Save the update to the database
                await CreditorServices.UpdateCreditorAsync(creditor);
                await LoadCreditorsAsync();
                await DialogHelper.ShowAlertAsync("Success", "Credit marked as unpaid again!");
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowAlertAsync("Error", $"Failed to mark as unpaid: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        public void ToggleNotes(Creditor creditor)
        {
            if (creditor == null) return;

            creditor.IsNotesExpanded = !creditor.IsNotesExpanded;

            // Refresh the item in the collection to update UI
            var index = FilteredCreditors.IndexOf(creditor);
            if (index >= 0)
            {
                FilteredCreditors[index] = creditor;
            }
        }
       
        [RelayCommand]
        public async Task ClearNotesAsync(Creditor creditor)
        {
            if (creditor == null) return;

            bool confirm = await DialogHelper.ShowConfirmAsync(
                "Clear Notes",
                "Are you sure you want to clear the notes?");

            if (!confirm) return;

            try
            {
                IsBusy = true;

                creditor.Notes = string.Empty;
                await CreditorServices.UpdateCreditorAsync(creditor);

                // Refresh UI
                var index = FilteredCreditors.IndexOf(creditor);
                if (index >= 0)
                {
                    FilteredCreditors[index] = creditor;
                }

                await DialogHelper.ShowAlertAsync("Success", "Notes cleared!");
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowAlertAsync("Error", $"Failed to clear notes: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
        private readonly Dictionary<int, System.Timers.Timer> _debounceTimers = new();
        private void DebounceAutoSave(Creditor creditor)
        {
            int key = creditor.Id;

            if (!_debounceTimers.ContainsKey(key))
            {
                var timer = new System.Timers.Timer(800) { AutoReset = false };
                timer.Elapsed += async (s, e) =>
                {
                    await SaveNotesAsync(creditor);
                };
                _debounceTimers[key] = timer;
            }

            _debounceTimers[key].Stop();
            _debounceTimers[key].Start();
        }

        private async Task SaveNotesAsync(Creditor creditor)
        {
            await CreditorServices.UpdateCreditorAsync(creditor);
            // Optional: show toast / feedback
            // await MainThread.InvokeOnMainThreadAsync(() => Toast.MakeText("Note saved").Show());
        }

    }
}
