using Syncfusion.Maui.Sliders;
using System.Globalization;
using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ReportsPage : ContentPage
{
	public ReportsPage(ReportsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }

}