using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using TindaKo.Models;

namespace TindaKo.ViewModel
{
    public partial class IncomingStocksPopupViewModel : ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<ProductDisplay> cartItems;
        public decimal TotalExpense => CartItems?.Sum(item => item.Subtotal) ?? 0;

        private IncomingStocksViewModel _parent;
        public IncomingStocksPopupViewModel(IncomingStocksViewModel parent)
        {
            _parent = parent;
            CartItems = _parent.CartList;
        }

        [RelayCommand]
        private void RemoveItem(ProductDisplay item)
        {
            if (_parent.CartList.Contains(item))
            {
                _parent.CartList.Remove(item);
                _parent.CartCount = _parent.CartList.Count; // 🔁 Sync cart count
                OnPropertyChanged(nameof(TotalExpense)); // manually notify the UI
            }
        }
    }
}
