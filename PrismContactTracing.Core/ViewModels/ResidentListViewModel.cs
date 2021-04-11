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
    public class ResidentListViewModel : BindableBase {

        private IDataListener _dataListener;
        private DataRowView _residentDataRowView;
        private DataTable _mainTable;
        private Cursor _cursorType;
        private string _residentId;
        private string _residentName;
        private string _firstName;
        private string _lastName;
        private string _idNumber;
        private string _purok;
        private string _address;
        private string _contactNumber;
        private string _eName;
        private string _eContact;
        private float _notifTransform = 500f;
        private string _notifMessage;
        private bool _isDialogOpen = false;
        private bool _isEnableEdit = false;
        private bool _isSavingEnable = false;
        private bool _onResidentReportLoaded = true; // To identify which table is displayed then apply proper filter through search input
        private float _spinnerEnable = 0f;
        private Visibility _isVisible;
        private bool _isRegisterDialogOpen;
        private bool _showConfirmDialog;

        public string ResidentId { get => _residentId; set { SetProperty(ref _residentId, value); } }
        public string FirstName { get => _firstName; set { SetProperty(ref _firstName, value); } }
        public string LastName { get => _lastName; set => _lastName = value; }
        public string IdNumber { get => _idNumber; set => _idNumber = value; }
        public string Purok { get => _purok; set => _purok = value; }
        public string Address { get => _address; set => _address = value; }
        public string ContactNumber { get => _contactNumber; set => _contactNumber = value; }
        public string EName { get => _eName; set => _eName = value; }
        public string EContact { get => _eContact; set => _eContact = value; }

        public DelegateCommand AddNewResidentCommand { get; private set; }
        public DelegateCommand ExecuteLoadResidentListCommand { get; private set; }
        public DelegateCommand ExecuteRegistrationDialogCommand { get; private set; }
        public DelegateCommand ExecuteApplyUpdateCommand { get; private set; }
        public DelegateCommand ExecuteSearchContentCommand { get; private set; }
        public DelegateCommand<object> ExecuteLoadResidentsReportCommand { get; private set; }
        public DelegateCommand ExecuteGenericDelegateOpenDialogCommand { get; private set; }
        public DelegateCommand ExecuteIsEnableEditCommand { get; private set; }
        public DelegateCommand ExecuteRefreshCommand { get; private set; }
        public DelegateCommand ExecuteArchiveResidentCommand { get; private set; }
        public DelegateCommand ExecuteShowConfirmDialogCommand { get; private set; }
        public DelegateCommand ExecuteInsertCommand { get; private set; }

        public DataTable MainDataTable {
            get => _mainTable;
            set { SetProperty(ref _mainTable, value); }
        }

        public string ResidentName {
            get => _residentName;
            set {
                SetProperty(ref _residentName, value);
                if (_onResidentReportLoaded) {
                    Task.Run(() => LoadResidentList(_residentName));
                } else {
                    //LoadResidentList(_residentName);
                }

                RaisePropertyChanged("MainDataTable");
            }
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

        public ResidentListViewModel(IDataListener dataListener) {
            _dataListener = dataListener;
            _dataListener.Procedure = "GetResidentsList";
            _dataListener.StartListen();

            DataListener.OnTableChangeEvent += RefreshTable;

            Task.Run(() => LoadResidentList(string.Empty));

            ExecuteRefreshCommand = new DelegateCommand(RefreshTable);
            ExecuteArchiveResidentCommand = new DelegateCommand(async () => await ArchiveRecord());
            ExecuteShowConfirmDialogCommand = new DelegateCommand(() => { ShowConfirmDialog = !ShowConfirmDialog; });
            ExecuteInsertCommand = new DelegateCommand(async () => await InsertResident());
            ExecuteRegistrationDialogCommand = new DelegateCommand(() => { IsRegisterDialogOpen = !IsRegisterDialogOpen; });
            ExecuteApplyUpdateCommand = new DelegateCommand(UpdateDb);
            ExecuteSearchContentCommand = new DelegateCommand(async () => await LoadResidentList(_residentName));
            ExecuteLoadResidentsReportCommand = new DelegateCommand<object>(async (p) => await LoadResidentList(string.Empty));
            ExecuteGenericDelegateOpenDialogCommand = new DelegateCommand(() => { IsRegisterDialogOpen = !IsRegisterDialogOpen; });
        }

        private async Task ArchiveRecord() {
            await Task.Run(() => {
                ShowConfirmDialog = !ShowConfirmDialog;

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new ArchiveQuery() {
                    TargetDataTable = MainDataTable,
                    Procedure = "ArchiveResident"
                });

                Task.Run(() => LoadResidentList(string.Empty));
            });
        }

        /// <param name="resident">Empty value will get all rows else otherwise</param>
        private async Task LoadResidentList(string resident) {
            await Task.Run(() => {
                SpinnerEnable = 1f;

                _cursorType = Cursors.Hand;
                RaisePropertyChanged("CursorType");

                IsVisible = Visibility.Visible;
                IsReadOnlyDataGrid = true;

                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                if (resident != string.Empty) {
                    parameter.Add(new KeyValuePair<string, string>("@m_resident", resident));
                }

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new SelectQuery() {
                    Procedure = resident == string.Empty ? "GetResidentsList" : "GetResident",
                    Parameters = parameter
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

        private async Task InsertResident() {
            await Task.Run(() => {
                SpinnerEnable = 1f;

                IsRegisterDialogOpen = !IsRegisterDialogOpen;

                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                parameter.Add(new KeyValuePair<string, string>("@m_firstname", _firstName));
                parameter.Add(new KeyValuePair<string, string>("@m_lastname", _lastName));
                parameter.Add(new KeyValuePair<string, string>("@m_purok", _purok));
                parameter.Add(new KeyValuePair<string, string>("@m_address", _address));
                parameter.Add(new KeyValuePair<string, string>("@m_contactnumber", _contactNumber));
                parameter.Add(new KeyValuePair<string, string>("@m_econtactnumber", _eName));
                parameter.Add(new KeyValuePair<string, string>("@m_econtactname", _eContact));

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new InsertQuery() {
                    Procedure = "InsertResident",
                    Parameters = parameter
                });

                Task.Run(() => LoadResidentList(string.Empty));
            });
        }

        private void UpdateDb() {
            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new UpdateQuery() { Procedure = "GetResidentsList" });

            Task.Run(() => LoadResidentList(string.Empty));

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
            Task.Run(() => LoadResidentList(string.Empty));
        }
    }
}