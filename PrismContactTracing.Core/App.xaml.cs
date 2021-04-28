using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using PrismContactTracing.Core.ViewModels;
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

            ViewModelLocationProvider.Register<LoginView, LoginViewModel>();
            ViewModelLocationProvider.Register<HomeView, HomeViewModel>();
            ViewModelLocationProvider.Register<ResidentReportView, ResidentReportViewModel>();
            ViewModelLocationProvider.Register<ResidentListView, ResidentListViewModel>();
            ViewModelLocationProvider.Register<ArchiveView, ArchiveViewModel>();
            ViewModelLocationProvider.Register<AdminView, AdminViewModel>();
            ViewModelLocationProvider.Register<SettingsView, SettingsViewModel>();
        }
    }
}
