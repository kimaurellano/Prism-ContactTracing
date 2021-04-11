using Prism.Commands;
using Prism.Mvvm;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Listener;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrismContactTracing.Core.ViewModels {
    public class AdminViewModel : BindableBase {
        private IDataListener _dataListener;
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

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Level { get; set; }

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

        public DataTable MainDataTable {
            get => _mainTable;
            set { SetProperty(ref _mainTable, value); }
        }

        public string ResidentName {
            get => _residentName;
            set { SetProperty(ref _residentName, value); }
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
            set { SetProperty(ref _cursorType, value); }
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

        public AdminViewModel(IDataListener dataListener) {
            _dataListener = dataListener;
            _dataListener.StartDbChangesListener();

            DataListener.OnTableChangeEvent += RefreshTable;

            Task.Run(() => LoadAdmins());

            ExecuteRefreshCommand = new DelegateCommand(RefreshTable);
            ExecuteDeleteAdminCommand = new DelegateCommand(async () => await DeleteAdmin());
            ExecuteShowConfirmDialogCommand = new DelegateCommand(() => { ShowConfirmDialog = !ShowConfirmDialog; });
            ExecuteInsertCommand = new DelegateCommand(async () => await InsertAdmin());
            ExecuteRegistrationDialogCommand = new DelegateCommand(() => { IsRegisterDialogOpen = !IsRegisterDialogOpen; });
            ExecuteApplyUpdateCommand = new DelegateCommand(UpdateDb);
            ExecuteLoadAdminsCommand = new DelegateCommand<object>(async (p) => await LoadAdmins());
        }

        private async Task DeleteAdmin() {
            await Task.Run(() => {
                ShowConfirmDialog = !ShowConfirmDialog;

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new CheckForMarks() {
                    TargetDataTable = MainDataTable,
                    Procedure = "DeleteAdmin",
                    ParameterName = "@m_key"
                });

                Task.Run(() => LoadAdmins());
            });
        }

        private async Task LoadAdmins() {
            await Task.Run(() => {
                SpinnerEnable = 1f;

                _cursorType = Cursors.Hand;
                RaisePropertyChanged("CursorType");

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
                SpinnerEnable = 1f;

                IsRegisterDialogOpen = !IsRegisterDialogOpen;

                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                parameter.Add(new KeyValuePair<string, string>("@m_username", Username));
                parameter.Add(new KeyValuePair<string, string>("@m_password", Password));
                parameter.Add(new KeyValuePair<string, string>("@m_level", Level));
                parameter.Add(new KeyValuePair<string, string>("@m_address", Address));
                parameter.Add(new KeyValuePair<string, string>("@m_first_name", FirstName));
                parameter.Add(new KeyValuePair<string, string>("@m_last_name", LastName));
                parameter.Add(new KeyValuePair<string, string>("@m_contact_number", ContactNumber));

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
    }
}