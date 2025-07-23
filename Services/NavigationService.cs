using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(Page page);
    }

    public class NavigationService : INavigationService
    {
        public Task NavigateToAsync(Page page)
        {
            return Application.Current.MainPage.Navigation.PushAsync(page);
        }
    }

}
