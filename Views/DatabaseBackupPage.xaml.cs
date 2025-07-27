using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class DatabaseBackupPage : ContentPage
{
	public DatabaseBackupPage(DBViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm
	}
}