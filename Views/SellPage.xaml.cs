using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class SellPage : ContentPage
{
	public SellPage(SellViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Kumpirmasyon",
                "Gusto mo bang kanselahin ang kasalukuyang transaksyon ng pagbebenta?",
                "Oo", "Hindi");


            if (answer)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        });

        return true; // prevent default behavior
    }
}