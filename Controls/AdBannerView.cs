using Plugin.MauiMtAdmob.Controls;
using Plugin.MauiMtAdmob.Extra;

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
            AdSize = BannerSize.Banner,
            HeightRequest = 50,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        adView.SetBinding(MTAdView.AdsIdProperty, new Binding(nameof(AdUnitId), source: this));
        adView.Loaded += (_, _) => adView.LoadAd(CollapsibleBannerMode.None);
        Content = adView;
    }
}
