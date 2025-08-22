using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class IncomingStocksDetailsPage : ContentPage
{
	public IncomingStocksDetailsPage(IncomingStocksLogsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}