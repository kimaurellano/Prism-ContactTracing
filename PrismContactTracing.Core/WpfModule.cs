using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PrismContactTracing.Core {
    class WpfModule : IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            // Makes request to ContentRegion to be replace with target view
            regionManager.RequestNavigate("ContentRegion", "YourView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            //containerRegistry.Register<IDbConnector, DbConnector>();

            //containerRegistry.RegisterForNavigation<LoginView>();
        }
    }
}
