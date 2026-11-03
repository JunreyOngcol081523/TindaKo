using Syncfusion.Maui.Toolkit.Popup;
using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class QuantityPopup : SfPopup
{
    public QuantityPopup(QuantityPopupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.RequestClose = () => Dismiss();
    }

    // select the whole number on focus, so typing replaces the default "1"
    private void OnQtyFocused(object? sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;

        Dispatcher.Dispatch(() =>
        {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text?.Length ?? 0;
        });
    }
}