using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using static SQLite.SQLite3;

namespace TindaKo.ViewModel
{
    public partial class BuyerViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Buyer> buyers;

        [ObservableProperty] private string newBuyerName;
        [ObservableProperty] private string newBuyerContact;
        [ObservableProperty] private string newBuyerAddress;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private Buyer selectedBuyer;
        public string SelectedBuyerName => SelectedBuyer?.Name;

        public Action<Buyer> OnBuyerSelected { get; set; }

        public BuyerViewModel()
        {
             _ = LoadBuyers();
        }

        public async Task LoadBuyers()
        {
            List<Buyer> result;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                result = await BuyerServices.GetBuyersAsync();
            }
            else
            {
                var allBuyers = await BuyerServices.GetBuyersAsync();
                result = allBuyers
                    .Where(b => b.Name != null &&
                                b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            Buyers = new ObservableCollection<Buyer>(result);
        }
        partial void OnSearchTextChanged(string value)
        {
            _ = LoadBuyers();
        }

        [RelayCommand]
        private void AddBuyer()
        {
            if (string.IsNullOrWhiteSpace(NewBuyerName)) return;

            var buyer = new Buyer
            {
                Name = NewBuyerName,
                ContactNumber = NewBuyerContact,
                Address = NewBuyerAddress
            };

            Buyers.Add(buyer);
            BuyerServices.AddBuyerAsync(buyer);
            // Reset fields
            NewBuyerName = NewBuyerContact = NewBuyerAddress = string.Empty;
        }

        [RelayCommand]
        private void SelectBuyer(Buyer buyer)
        {
            SelectedBuyer = buyer;
            OnPropertyChanged(nameof(SelectedBuyerName));
        }

        [RelayCommand]
        private async Task ConfirmSelection()
        {
            if (SelectedBuyer is null)
            {
                await ToastHelper.ShowError("Please select a buyer.");
                return;
            }

            OnBuyerSelected?.Invoke(SelectedBuyer);
            await App.Navigator.Navigation.PopModalAsync();

        }
        [RelayCommand]
        private async Task CloseBuyerAsync()
        {
            await App.Navigator.Navigation.PopModalAsync();
        }
    }

}
