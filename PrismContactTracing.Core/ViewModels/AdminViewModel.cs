using MySql.Data.MySqlClient;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Interface;
using PrismContactTracing.Core.Listener;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrismContactTracing.Core.ViewModels {
    public class AdminViewModel : BindableBase, INavigationAware {
        private IDataListener _dataListener;
        private IRegionManager _regionManager;
        private IDbConnector _dbConnector;
        private DataRowView _residentDataRowView;
        private DataTable _mainTable;
        private Cursor _cursorType;
        private string _residentName;
        private float _notifTransform = 500f;
        private string _notifMessage;
        private bool _isEnableEdit = false;
        private bool _isSavingEnable = false;
        private float _spinnerEnable = 0f;
        private Visibility _isVisible;
        private bool _isRegisterDialogOpen;
        private bool _showConfirmDialog;
        private bool _isAllFieldsComplete;
        private string _inputWarning;
        private string _firstName;
        private string _lastName;
        private string _address;
        private string _contactNumber;
        private string _password;
        private string _username;
        private string _level;
        private bool _showSecurityConfirmationDialog;

        #region Delegates
        public DelegateCommand AddNewResidentCommand { get; private set; }
        public DelegateCommand ExecuteLoadResidentListCommand { get; private set; }
        public DelegateCommand ExecuteRegistrationDialogCommand { get; private set; }
        public DelegateCommand ExecuteApplyUpdateCommand { get; private set; }
        public DelegateCommand ExecuteSearchContentCommand { get; private set; }
        public DelegateCommand<object> ExecuteLoadAdminsCommand { get; private set; }
        public DelegateCommand ExecuteGenericDelegateOpenDialogCommand { get; private set; }
        public DelegateCommand ExecuteIsEnableEditCommand { get; private set; }
        public DelegateCommand ExecuteRefreshCommand { get; private set; }
        public DelegateCommand ExecuteDeleteAdminCommand { get; private set; }
        public DelegateCommand ExecuteShowConfirmDialogCommand { get; private set; }
        public DelegateCommand ExecuteInsertCommand { get; private set; }
        public DelegateCommand<object> ExecuteConfirmAccess { get; private set; }
        public DelegateCommand ExecuteCancelAccess { get; private set; }
        #endregion Delegates

        #region GetterSetter
        public string FirstName { get => _firstName; set { SetProperty(ref _firstName, value); CheckFields(); } }
        public string LastName { get => _lastName; set { SetProperty(ref _lastName, value); CheckFields(); } }
        public string Address { get => _address; set { SetProperty(ref _address, value); CheckFields(); } }
        public string ContactNumber { get => _contactNumber; set { SetProperty(ref _contactNumber, value); CheckFields(); } }
        public string Password { get => _password; set { SetProperty(ref _password, value); CheckFields(); } }
        public string Username { get => _username; set { SetProperty(ref _username, value); CheckFields(); } }
        public string Level { get => _level; set { SetProperty(ref _level, value); CheckFields(); } }

        public string InputWarning {
            get => _inputWarning;
            set { SetProperty(ref _inputWarning, value); RaisePropertyChanged("InputWarning"); }
        }

        public DataTable MainDataTable {
            get => _mainTable;
            set { SetProperty(ref _mainTable, value); }
        }

        public string ResidentName {
            get => _residentName;
            set { SetProperty(ref _residentName, value); }
        }

        public bool ShowSecurityConfirmationDialog {
            get => _showSecurityConfirmationDialog;
            set { SetProperty(ref _showSecurityConfirmationDialog, value); RaisePropertyChanged("ShowSecurityConfirmationDialog"); }
        }

        public bool ShowConfirmDialog {
            get => _showConfirmDialog;
            set { SetProperty(ref _showConfirmDialog, value); }
        }

        public bool IsRegisterDialogOpen {
            get => _isRegisterDialogOpen;
            set { SetProperty(ref _isRegisterDialogOpen, value); }
        }

        public Visibility IsVisible {
            get => _isVisible;
            set { SetProperty(ref _isVisible, value); }
        }

        public float SpinnerEnable {
            get => _spinnerEnable;
            set { SetProperty(ref _spinnerEnable, value); }
        }

        public bool IsReadOnlyDataGrid {
            get => _isEnableEdit;
            set { SetProperty(ref _isEnableEdit, value); }
        }

        public bool IsSavingEnabled {
            get => _isSavingEnable;
            set { SetProperty(ref _isSavingEnable, value); }
        }

        // Visual representation if datagrid row can be selected 
        public Cursor CursorType {
            get => _cursorType;
            set { SetProperty(ref _cursorType, value); RaisePropertyChanged("CursorType"); }
        }

        public string NotifMessage {
            get => _notifMessage;
            set { SetProperty(ref _notifMessage, value); }
        }

        public float NotifTransform {
            get => _notifTransform;
            set { SetProperty(ref _notifTransform, value); }
        }

        public DataRowView ResidentDataRowView {
            get => _residentDataRowView;
            set { SetProperty(ref _residentDataRowView, value); }
        }

        public bool IsAllFieldsComplete {
            get => _isAllFieldsComplete;
            set { SetProperty(ref _isAllFieldsComplete, value); RaisePropertyChanged("IsAllFieldsComplete"); }
        }
        #endregion GetterSetter

        public AdminViewModel(IDataListener dataListener, IRegionManager regionManager, IDbConnector dbConnector) {
            ShowSecurityConfirmationDialog = true;

            _dbConnector = dbConnector;
            _regionManager = regionManager;
            _dataListener = dataListener;
            _dataListener.StartDbChangesListener();

            DataListener.OnTableChangeEvent += RefreshTable;

            Task.Run(() => LoadAdmins());

            ExecuteCancelAccess = new DelegateCommand(() => { ShowSecurityConfirmationDialog = !ShowSecurityConfirmationDialog; NavigateTo("ResidentListView"); });
            ExecuteConfirmAccess = new DelegateCommand<object>((passwordParameter) => { ValidateUser(passwordParameter); });
            ExecuteRefreshCommand = new DelegateCommand(RefreshTable);
            ExecuteDeleteAdminCommand = new DelegateCommand(async () => await DeleteAdmin());
            ExecuteShowConfirmDialogCommand = new DelegateCommand(() => { ShowConfirmDialog = !ShowConfirmDialog; });
            ExecuteInsertCommand = new DelegateCommand(async () => await InsertAdmin());
            ExecuteRegistrationDialogCommand = new DelegateCommand(() => { IsRegisterDialogOpen = !IsRegisterDialogOpen; });
            ExecuteApplyUpdateCommand = new DelegateCommand(UpdateDb);
            ExecuteLoadAdminsCommand = new DelegateCommand<object>(async (p) => await LoadAdmins());
        }

        private void CheckFields() {
            IsAllFieldsComplete = (
                FirstName != null && LastName != null && Address != null && ContactNumber != null && Password != null && Username != null &&
                FirstName != string.Empty && LastName != string.Empty && Address != string.Empty && ContactNumber != string.Empty && Password != string.Empty && Username != string.Empty);
        }

        private async Task DeleteAdmin() {
            await Task.Run(() => {
                ShowConfirmDialog = !ShowConfirmDialog;

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new CheckForMarks() {
                    TargetDataTable = MainDataTable,
                    Procedure = "DeleteAdmin",
                    ParameterName = "@m_key",
                    MarkType = CheckForMarks.Mark.Delete
                });

                Task.Run(() => LoadAdmins());
            });
        }

        private async Task LoadAdmins() {
            await Task.Run(() => {
                SpinnerEnable = 1f;

                CursorType = Cursors.Hand;

                IsVisible = Visibility.Visible;
                IsReadOnlyDataGrid = true;

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new GetDataQuery() {
                    Procedure = "GetAdmins"
                });

                // Avoid clearing on load
                if (_mainTable != null) {
                    _mainTable.Clear();
                }

                _mainTable = queryStrategy.MainDataTable;

                // Avoiding the main table not to update
                RaisePropertyChanged("MainDataTable");
            });

            SpinnerEnable = 0f;
        }

        private async Task InsertAdmin() {
            await Task.Run(() => {
                if (!Validate(ContactNumber)) {
                    return;
                }

                SpinnerEnable = 1f;

                IsRegisterDialogOpen = !IsRegisterDialogOpen;

                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                parameter.Add(new KeyValuePair<string, string>("@m_username", Username));
                parameter.Add(new KeyValuePair<string, string>("@m_password", Password));
                parameter.Add(new KeyValuePair<string, string>("@m_level", Level));
                parameter.Add(new KeyValuePair<string, string>("@m_address", Address));
                parameter.Add(new KeyValuePair<string, string>("@m_first_name", Format(FirstName)));
                parameter.Add(new KeyValuePair<string, string>("@m_last_name", Format(LastName)));
                parameter.Add(new KeyValuePair<string, string>("@m_contact_number", ContactNumber));

                // Clear on submit
                Username = string.Empty;
                Password = string.Empty;
                Level = string.Empty;
                Address = string.Empty;
                FirstName = string.Empty;
                LastName = string.Empty;
                ContactNumber = string.Empty;

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new InsertQuery() {
                    Procedure = "InsertAdmin",
                    Parameters = parameter
                });

                Task.Run(() => LoadAdmins());
            });
        }

        private void UpdateDb() {
            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new UpdateQuery() { Procedure = "GetAdmins", TargetDataTable = MainDataTable });

            Task.Run(() => LoadAdmins());

            Show("Updated successfully", 3000);
        }

        public void Show(string message, int time) {
            NotifMessage = message;

            Application.Current.Dispatcher.Invoke(async () => {
                await Task.Run(() => {
                    while (NotifTransform > -10f) {
                        NotifTransform -= 1f;
                        Thread.Sleep(1);
                    }

                    Thread.Sleep(time);

                    while (NotifTransform < 200f) {
                        NotifTransform += 1f;
                        Thread.Sleep(1);
                    }
                });
            });
        }

        private void RefreshTable() {
            Task.Run(() => LoadAdmins());
        }

        private string Format(string text) {
            string firstChar = text.Substring(0, 1).ToUpper();
            string noFirstChar = text.Substring(1, text.Length - 1).ToLower();

            return firstChar + noFirstChar;
        }

        private bool Validate(string input) {
            bool hasNonDigit = false;
            foreach (char character in input.ToCharArray()) {
                if (!char.IsDigit(character)) {
                    hasNonDigit = true;
                }
            }

            if (!input.Substring(0, 2).Equals("09") || input.Length != 11 || hasNonDigit) {
                InputWarning = "Invalid contact number format. Please use 11-digit 09xxxxxxxxx.";
                return false;
            }

            return true;
        }

        private void NavigateTo(string page) {
            if (page != null) {
                _regionManager.RequestNavigate("ContentType", page);
            }
        }

        private void ValidateUser(object password) {
            _dbConnector.Connect();

            MySqlCommand cmd = new MySqlCommand("GetUser", _dbConnector.DbConnectionInstance) {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@m_username", Persistent.LoggedUser);
            cmd.Parameters.AddWithValue("@m_password", ((PasswordBox)password).Password);
            cmd.Parameters.Add("@totalcount", MySqlDbType.Int32);
            cmd.Parameters["@totalcount"].Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            _dbConnector.Disconnect();

            int count = (int)cmd.Parameters["@totalcount"].Value;

            if (count == 0) {
                MessageBox.Show("Invalid credentials.", "Access rejected", MessageBoxButton.OK);

                return;
            }

            Show("Access granted", 1500);

            ShowSecurityConfirmationDialog = !ShowSecurityConfirmationDialog;
        }

        public void OnNavigatedTo(NavigationContext navigationContext) { return; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return false; }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            var singleView = _regionManager.Regions["ContentType"].ActiveViews.FirstOrDefault();
            _regionManager.Regions["ContentType"].Deactivate(singleView);
        }
    }
}