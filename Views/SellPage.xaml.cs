using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class SellPage : ContentPage
{
	public SellPage(SellViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}