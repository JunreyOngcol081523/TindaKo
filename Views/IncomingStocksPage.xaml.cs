using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class IncomingStocksPage : ContentPage
{
	public IncomingStocksPage(IncomingStocksViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}