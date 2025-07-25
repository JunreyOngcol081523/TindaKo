using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class SalesDetailsPage : ContentPage
{
    public SalesDetailsPage(SalesDetailsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}


}