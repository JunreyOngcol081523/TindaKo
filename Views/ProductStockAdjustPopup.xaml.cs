using CommunityToolkit.Maui.Views;
using TindaKo.Models;
using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ProductStockAdjustPopup : ContentPage
{
	public ProductStockAdjustPopup(ProductAdjustViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}