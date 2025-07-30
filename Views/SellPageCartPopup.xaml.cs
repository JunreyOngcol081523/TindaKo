using Syncfusion.Maui.Toolkit.Popup;
using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class SellPageCartPopup : SfPopup
{
	public SellPageCartPopup(SellViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}