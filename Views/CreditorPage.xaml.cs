using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class CreditorPage : ContentPage
{
	public CreditorPage(int initialSegment = 0)
	{
		InitializeComponent();
        var vm = new CreditorViewModel();
        vm.SelectedSegmentIndex = initialSegment;
        BindingContext = vm;
    }
}