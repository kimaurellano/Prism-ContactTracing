using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Interface;
using PrismContactTracing.Core.Listener;
using PrismContactTracing.WPF.Views;

namespace PrismContactTracing.Core {
    class WpfModule : IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            // Makes request to ContentRegion to be replace with target view
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(LoginView));
            regionManager.RegisterViewWithRegion("ContentType", typeof(ResidentListView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.Register<IDbConnector, DbConnector>();
            containerRegistry.Register<IDataListener, DataListener>();

            containerRegistry.RegisterForNavigation<ResidentReportView>();
            containerRegistry.RegisterForNavigation<ResidentListView>();
            containerRegistry.RegisterForNavigation<ArchiveView>();
            containerRegistry.RegisterForNavigation<LoginView>();
            containerRegistry.RegisterForNavigation<HomeView>();
            containerRegistry.RegisterForNavigation<AdminView>();
        }
    }
}
