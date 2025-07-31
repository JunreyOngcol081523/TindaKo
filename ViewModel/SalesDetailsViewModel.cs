using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{

    public partial class SalesDetailsViewModel:ObservableObject
    {
       [ObservableProperty] private TransactionDisplay transDisplay;
        public SalesDetailsViewModel(TransactionDisplay _transactionDisplay) {

            SoldItems = new ObservableCollection<Sell>(); 
            transDisplay = _transactionDisplay;
           _= LoadSellByTransactionID();
        }
        [ObservableProperty] private ObservableCollection<Sell> soldItems;


        private async Task LoadSellByTransactionID()
        {
            var transID = TransDisplay.TransactionId;
            List<Sell> sellItems = await TransactionServices.GetSellsAsync(transID);
            SoldItems.Clear();
            foreach (Sell sell in sellItems) { 
                SoldItems.Add(sell);
            }
        }
        [RelayCommand]
        private async Task CloseDetailsAsync()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
        //[ObservableProperty] private int sellId;
        //[ObservableProperty] private int transactionId;
        //[ObservableProperty] private int productID;// Foreign key to Transaction table
        //[ObservableProperty] private string productName;
        //[ObservableProperty] private string category;
        //[ObservableProperty] private decimal price;
        //[ObservableProperty] private decimal qty;
        //[ObservableProperty] private decimal soldSubtotal;