using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using PrismContactTracing.WPF.Views;
using System.Windows;

namespace PrismContactTracing.Core {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication {
        protected override Window CreateShell() {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) {

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
            moduleCatalog.AddModule<WpfModule>();
        }

        protected override void ConfigureViewModelLocator() {
            base.ConfigureViewModelLocator();
        }
    }
}
