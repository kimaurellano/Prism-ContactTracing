using Microsoft.Data.Sqlite;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.Interface;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PrismContactTracing.Core.ViewModels {
    public class LoginViewModel : BindableBase {

        private IRegionManager _regionManager;
        private IDbConnector _dbConnector;
        private string _username;
        private bool _isAuth;
        private float _isVisible;

        public DelegateCommand<object> ExecuteGenericDelegateCommand { get; private set; }

        public string Username {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public bool IsAuth {
            get => !_isAuth;
            set { SetProperty(ref _isAuth, value, "IsVisible"); }
        }

        public float IsVisible {
            get => _isVisible;
            set { SetProperty(ref _isVisible, value); }
        }

        public LoginViewModel(IRegionManager regionManager, IDbConnector dbConnector) {
            _regionManager = regionManager;
            _dbConnector = dbConnector;

            ExecuteGenericDelegateCommand = new DelegateCommand<object>(async (p) => await ExecuteGeneric(p));
        }

        private async Task ExecuteGeneric(object parameter) {
            var result = await Task.Run(() => {
                IsVisible = 1;

                _dbConnector.Connect();

                var value = (PasswordBox)parameter;

                if(Username == string.Empty || value.Password == string.Empty) {
                    MessageBox.Show("Missing information", "Login error", MessageBoxButton.OK);
                    return string.Empty;
                }

                SqliteCommand cmd = new SqliteCommand("select count(*) from user where username=$username and password=$password", _dbConnector.DbConnectionInstance);
                cmd.Parameters.AddWithValue("$username", Username);
                cmd.Parameters.AddWithValue("$password", value.Password);
                var firstColumn = cmd.ExecuteScalar().ToString();

                _dbConnector.Disconnect();

                if (firstColumn == "0") {
                    MessageBox.Show("Invalid credentials", "Login error", MessageBoxButton.OK);
                    return string.Empty;
                }

                return "HomeView";
            });

            IsVisible = 0;

            Navigate(result);
        }

        private void Navigate(string navigatePath) {
            if (navigatePath != null) {
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
            }
        }
    }
}
