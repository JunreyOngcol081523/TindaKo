namespace TindaKo.Views;
using Syncfusion.Maui.Toolkit.Popup;
using TindaKo.ViewModel;

public partial class IncomingStocksPopup : SfPopup
{
	public IncomingStocksPopup(IncomingStocksPopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		//IsFullScreen = true;
	}
	
}