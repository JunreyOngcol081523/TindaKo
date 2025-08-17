using Syncfusion.Maui.Toolkit.Popup;
using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class FastSellingPopup : SfPopup
{
	public FastSellingPopup()
	{
		InitializeComponent();
		BindingContext = new FastSellingProductViewModel();
    }
}