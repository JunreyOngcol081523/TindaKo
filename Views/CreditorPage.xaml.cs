using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class CreditorPage : ContentPage
{
	public CreditorPage()
	{
		InitializeComponent();
		BindingContext = new CreditorViewModel();
	}
}