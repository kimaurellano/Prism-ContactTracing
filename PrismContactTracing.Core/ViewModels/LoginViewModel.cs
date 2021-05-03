using MySql.Data.MySqlClient;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.Interface;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PrismContactTracing.Core.ViewModels {
    public class LoginViewModel : BindableBase {

        private IRegionManager _regionManager;
        private IDbConnector _dbConnector;
        private string _username;
        private float _spinnerEnable;
        private bool _allowLogin;

        public DelegateCommand<object> ExecuteGenericDelegateCommand { get; private set; }

        public string Username {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public bool AllowLogin { 
            get => _allowLogin; 
            set { SetProperty(ref _allowLogin, value); RaisePropertyChanged("AllowLogin"); } 
        }

        public float SpinnerEnable {
            get => _spinnerEnable;
            set { SetProperty(ref _spinnerEnable, value); }
        }

        public LoginViewModel(IRegionManager regionManager, IDbConnector dbConnector) {
            _regionManager = regionManager;
            _dbConnector = dbConnector;

            ExecuteGenericDelegateCommand = new DelegateCommand<object>(async (p) => await ExecuteGeneric(p));

            AllowLogin = true;
        }

        private async Task ExecuteGeneric(object parameter) {
            var result = await Task.Run(() => {
                SpinnerEnable = 1;

                AllowLogin = !AllowLogin;

                _dbConnector.Connect();

                var value = (PasswordBox)parameter;

                if(Username == string.Empty || value.Password == string.Empty) {
                    MessageBox.Show("Missing information", "Login error", MessageBoxButton.OK);
                    return string.Empty;
                }

                MySqlCommand cmd = new MySqlCommand("GetUser", _dbConnector.DbConnectionInstance) {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@m_username", Username);
                cmd.Parameters.AddWithValue("@m_password", value.Password);
                cmd.Parameters.Add("@totalcount", MySqlDbType.Int32);
                cmd.Parameters["@totalcount"].Direction = System.Data.ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                _dbConnector.Disconnect();

                int count = (int)cmd.Parameters["@totalcount"].Value;

                if (count == 0) {
                    MessageBox.Show("Invalid credentials", "Login error", MessageBoxButton.OK);
                    return string.Empty;
                }

                return "HomeView";
            });

            SpinnerEnable = 0;

            AllowLogin = !AllowLogin;

            Navigate(result);
        }

        private void Navigate(string navigatePath) {
            if (navigatePath != null) {
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
            }
        }
    }
}
