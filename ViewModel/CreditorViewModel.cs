using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TindaKo.Models;
using TindaKo.Services;

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
        public CreditorViewModel()
        {
            _ = LoadCreditorsAsync();
            SegmentOptions = new ObservableCollection<string>
        {
            "All", "Past Due", "Paid", "Unpaid"
        };
            SelectedSegmentIndex = 0; // default to "All"
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

            // Save the update to the database
            await CreditorServices.UpdateCreditorAsync(creditor);

            // Reload the list with the updated values
            await LoadCreditorsAsync();
        }

    }
}
