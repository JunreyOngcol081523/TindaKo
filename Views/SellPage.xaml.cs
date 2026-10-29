using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class SellPage : ContentPage
{
	public SellPage(SellViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as SellViewModel)?.InitializeAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bool answer = await App.Navigator.DisplayAlert(
                "Kumpirmasyon",
                "Gusto mo bang kanselahin ang kasalukuyang transaksyon ng pagbebenta?",
                "Oo", "Hindi");


            if (answer)
            {
                await App.Navigator.Navigation.PopAsync();
            }
        });

        return true; // prevent default behavior
    }
}