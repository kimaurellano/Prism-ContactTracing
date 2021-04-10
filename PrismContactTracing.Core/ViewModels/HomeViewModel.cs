using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PrismContactTracing.Core.ViewModels {
    public class HomeViewModel : BindableBase {

        private IRegionManager _regionManager;

        public DelegateCommand<string> NavigateToCommand { get; private set; }

        public HomeViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;

            NavigateToCommand = new DelegateCommand<string>(NavigateTo);
        }

        private void NavigateTo(string page) {
            if (page != null) {
                _regionManager.RequestNavigate("ContentType", page);
            }

            page = page.Replace("View", string.Empty);
        }
    }
}