using Plugin.MauiMtAdmob.Controls;

namespace TindaKo.Controls;

public sealed class AdBannerView : ContentView
{
    public static readonly BindableProperty AdUnitIdProperty = BindableProperty.Create(
        nameof(AdUnitId),
        typeof(string),
        typeof(AdBannerView),
        string.Empty);

    public string AdUnitId
    {
        get => (string)GetValue(AdUnitIdProperty);
        set => SetValue(AdUnitIdProperty, value);
    }

    public AdBannerView()
    {
        var adView = new MTAdView
        {
            HeightRequest = 50,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        adView.SetBinding(MTAdView.AdsIdProperty, new Binding(nameof(AdUnitId), source: this));
        Content = adView;
    }
}
