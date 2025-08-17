using Syncfusion.Maui.Toolkit.Popup;

namespace TindaKo.Views;

public partial class LowStockPopup : SfPopup
{
	public LowStockPopup()
	{
		InitializeComponent();
		BindingContext = new ViewModel.LowStockPopupViewModel();
    }
}