using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;
using static SQLite.SQLite3;

namespace TindaKo.ViewModel
{
    public partial class SalesViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<TransactionDisplay> transactionDisplay;
        [ObservableProperty] private ObservableCollection<string> transactionType;
        [ObservableProperty] private string selectedTransactionType;
        [ObservableProperty] private bool isFilterEnabled;
        [ObservableProperty] private DateTime dateFrom;
        [ObservableProperty] private DateTime dateTo;
        [ObservableProperty] private decimal salesAmount;
        private string _buyerName;
        public string BuyerName
        {
            get => _buyerName;
            set => SetProperty(ref _buyerName, value); // Assuming you're using MVVM Toolkit
        }
        public SalesViewModel()
        {
            transactionType = new ObservableCollection<string> {"","Cash","Credit"};
            SelectedTransactionType = "Cash";
            dateFrom = dateTo = DateTime.Now;
            
        }

        [RelayCommand]
        public async Task LoadTransactionAsync()
        {
            List<Transaction> rawTransactions;

            if (IsFilterEnabled)
            {
                rawTransactions = await TransactionServices.GetFilteredTransactionsAsync(true, DateFrom, DateTo, SelectedTransactionType);
            }
            else
            {
                rawTransactions = await TransactionServices.GetFilteredTransactionsAsync(false, DateTime.MinValue, DateTime.MaxValue, string.Empty);
            }

            var displayList = new List<TransactionDisplay>();
            SalesAmount = 0;
            foreach (var tx in rawTransactions)
            {
                var buyer = await BuyerServices.GetBuyerAsync(tx.BuyerID); // Fetch buyer
                displayList.Add(new TransactionDisplay
                {
                    TransactionId = tx.TransactionId,
                    TransactionType = tx.TransactionType,
                    DateTimeTransacted = tx.DateTimeTransacted,
                    TotalAmount = tx.TotalAmount,
                    Username = tx.Username,
                    BuyerID = tx.BuyerID,
                    BuyerName = buyer?.Name ?? ""
                });
                SalesAmount += tx.TotalAmount;
            }

            TransactionDisplay = new ObservableCollection<TransactionDisplay>(displayList);
        }

        [RelayCommand]
        private async Task ItemTapped(TransactionDisplay selectedTransaction)
        {
            if (selectedTransaction == null)
            {
                Debug.WriteLine("[ItemTapped:SalesPage] selectedTransaction null");
                return;
            }
                
            SalesDetailsViewModel vm = new SalesDetailsViewModel(selectedTransaction);
            SalesDetailsPage salesDetailsPage = new SalesDetailsPage(vm);
            await App.Navigator.Navigation.PushModalAsync(salesDetailsPage);
        }
    }
}
