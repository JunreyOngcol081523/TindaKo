using System.Globalization;

namespace TindaKo.Views;

public partial class IncomingStocksLogsPage : ContentPage
{

    public IncomingStocksLogsPage()
	{
		InitializeComponent();

        BindingContext = new ViewModel.IncomingStocksLogsViewModel();
    }
}