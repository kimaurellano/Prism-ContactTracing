using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace PrismContactTracing.Core.ViewModels {
    public class HomeViewModel : BindableBase {

        private IRegionManager _regionManager;
        private ObservableCollection<ResidentModel> _residents = new ObservableCollection<ResidentModel>();
        private DataTable _mainTable;
        private DataRowView _residentDataRowView;
        private Cursor _cursorType;
        private string _residentName;
        private string _firstName;
        private string _lastName;
        private string _idNumber;
        private string _purok;
        private string _address;
        private string _contactNumber;
        private string _eName;
        private string _eContact;
        private string _searchType;
        private bool _isDialogOpen = false;
        private bool _isEnableEdit = false;
        private bool _isSavingEnable = false;
        private bool _onResidentReportLoaded = true; // To identify which table is displayed then apply proper filter through search input
        private Visibility _isVisible;
        private Visibility _backVisibility;
        private Visibility _dataModifierEnable;

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
        public DelegateCommand ExecuteLoadResidentsReportCommand { get; private set; }
        public DelegateCommand ExecuteGenericDelegateOpenDialogCommand { get; private set; }
        public DelegateCommand ExecuteIsEnableEditCommand { get; private set; }
        public DelegateCommand ExecuteInsertCommand { get; private set; }

        public DataTable MainDataTable {
            get => _mainTable;
            set { SetProperty(ref _mainTable, value); }
        }

        public ObservableCollection<ResidentModel> Residents {
            get => _residents;
            set { SetProperty(ref _residents, value); }
        }

        public string ResidentName {
            get => _residentName;
            set {
                SetProperty(ref _residentName, value);
                if (_onResidentReportLoaded) {
                    LoadResidentReport(_residentName);
                } else {
                    LoadResidentList(_residentName);
                }
                
                RaisePropertyChanged("MainDataTable");
            }
        }

        public string SearchType {
            get => _searchType;
            set {
                SetProperty(ref _searchType, value);
            }
        }

        public bool IsDialogOpen {
            get => _isDialogOpen;
            set { SetProperty(ref _isDialogOpen, value); }
        }

        public Visibility BackVisibility {
            get => _backVisibility;
            set { SetProperty(ref _backVisibility, value); }
        }

        public Visibility IsVisible {
            get => _isVisible;
            set { SetProperty(ref _isVisible, value); }
        }

        public Visibility DataModifierEnable {
            get => _dataModifierEnable;
            set { SetProperty(ref _dataModifierEnable, value); }
        }

        public bool IsReadOnlyDataGrid {
            get => _isEnableEdit;
            set { SetProperty(ref _isEnableEdit, value); }
        }

        public bool IsSavingEnabled {
            get => _isSavingEnable;
            set { SetProperty(ref _isSavingEnable, value); }
        }

        public bool OnResidentReportLoaded { 
            get => _onResidentReportLoaded;
            set { SetProperty(ref _onResidentReportLoaded, value); } 
        }

        // Visual representation if datagrid row can be selected 
        public Cursor CursorType {
            get => _cursorType;
            set { SetProperty(ref _cursorType, value); }
        }

        public DataRowView ResidentDataRowView {
            get => _residentDataRowView;
            set {
                if (value != null) {
                    SetProperty(ref _residentDataRowView, value);
                    if (_onResidentReportLoaded) {
                        LoadCloseContactTrace(_residentDataRowView.Row["First Name"].ToString(), _residentDataRowView.Row["Last Name"].ToString(), _residentDataRowView.Row["Time In"].ToString());
                        RaisePropertyChanged("MainDataTable");
                        IsVisible = Visibility.Hidden;
                        BackVisibility = Visibility.Visible;
                    }
                }
            }
        }

        public HomeViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;

            //Start read serial data

            LoadResidentReport(string.Empty);

            ExecuteLoadResidentListCommand = new DelegateCommand(() => LoadResidentList(string.Empty));
            ExecuteLoadResidentsReportCommand = new DelegateCommand(() => LoadResidentReport(string.Empty));
            ExecuteGenericDelegateOpenDialogCommand = new DelegateCommand(() => { IsDialogOpen = !IsDialogOpen; });
            ExecuteIsEnableEditCommand = new DelegateCommand(UpdateDb);
            ExecuteInsertCommand = new DelegateCommand(InsertResident);
        }

        private void InsertResident() {
            IsDialogOpen = !IsDialogOpen;

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new InsertQuery() { 
                Query = $"insert into Resident values (NULL,'{_firstName}','{_lastName}','{_purok}','{_contactNumber}','{_address}','{_eContact}','{_eName}')"
            });

            LoadResidentReport(string.Empty);
        }

        /// <param name="resident">Empty value will get all rows else otherwise</param>
        private void LoadResidentReport(string resident) {
            _cursorType = Cursors.Hand;
            RaisePropertyChanged("CursorType");

            OnResidentReportLoaded = true;
            DataModifierEnable = Visibility.Hidden;

            IsVisible = Visibility.Visible;
            BackVisibility = Visibility.Hidden;
            IsReadOnlyDataGrid = true;

            string query;
            if (resident == string.Empty) {
                query = "select rct.TIME_IN as 'Time In', r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', r.ADDRESS as 'Address', r.CONTACT_NUMBER as 'Contact Number', r.EMERGENCY_CONTACT_NUMBER as 'Emergency Contact Number', r.EMERGENCY_CONTACT_NAME as 'Emergency Contact Name' from Resident r left join ResidentContactTrace rct on r.RESIDENT_KEY = rct.RESIDENT_KEY where rct.TIME_IN <> ''";
            } else {
                query = $"select rct.TIME_IN as 'Time In', r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', r.ADDRESS as 'Address', r.CONTACT_NUMBER as 'Contact Number', r.EMERGENCY_CONTACT_NUMBER as 'Emergency Contact Number', r.EMERGENCY_CONTACT_NAME as 'Emergency Contact Name' from Resident r left join ResidentContactTrace rct on r.RESIDENT_KEY = rct.RESIDENT_KEY where r.FIRST_NAME LIKE '%{resident}%' and rct.TIME_IN <> ''";
            }

            SearchType = $"Reported list of resident's Time-ins";

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() {
                Query = query
            });

            // Avoid clearing on load
            if(_mainTable != null) {
                _mainTable.Clear();
            }

            _mainTable = queryStrategy.MainDataTable;

            // Avoiding the main table not to update
            RaisePropertyChanged("MainDataTable");
        }

        private void LoadCloseContactTrace(string firstName, string lastName, string timeIn) {
            _cursorType = Cursors.Arrow;
            RaisePropertyChanged("CursorType");

            SearchType = $"Close contact of {firstName} {lastName} @ {timeIn}";

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() {
                Query = $"select rct.TIME_IN as 'Time In', r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', r.ADDRESS as 'Address', r.CONTACT_NUMBER as 'Contact Number', r.EMERGENCY_CONTACT_NUMBER as 'Emergency Contact Number', r.EMERGENCY_CONTACT_NAME as 'Emergency Contact Name' from Resident r left join ResidentContactTrace rct on r.RESIDENT_KEY = rct.RESIDENT_KEY where rct.TIME_IN LIKE '%{timeIn.Split(' ')[0]}%'"
            });

            _mainTable.Clear();
            _mainTable = queryStrategy.MainDataTable;
        }

        /// <param name="resident">Empty value will get all rows else otherwise</param>
        private void LoadResidentList(string resident) {
            _cursorType = Cursors.Arrow;
            RaisePropertyChanged("CursorType");

            OnResidentReportLoaded = false;
            DataModifierEnable = Visibility.Visible;

            IsVisible = Visibility.Visible;
            BackVisibility = Visibility.Hidden;
            IsReadOnlyDataGrid = false;

            SearchType = $"Resident list";

            string query;
            if(resident == string.Empty) {
                query = $"select r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', r.ADDRESS as 'Address', r.CONTACT_NUMBER as 'Contact Number', r.EMERGENCY_CONTACT_NUMBER as 'Emergency Contact Number', r.EMERGENCY_CONTACT_NAME as 'Emergency Contact Name' from Resident r";
            } else {
                query = $"select r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', r.ADDRESS as 'Address', r.CONTACT_NUMBER as 'Contact Number', r.EMERGENCY_CONTACT_NUMBER as 'Emergency Contact Number', r.EMERGENCY_CONTACT_NAME as 'Emergency Contact Name' from Resident r where r.FIRST_NAME='{resident}'";
            }

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() {
                Query = query
            });

            _mainTable.Clear();
            _mainTable = queryStrategy.MainDataTable;

            RaisePropertyChanged("MainDataTable");
        }

        private void UpdateDb() {
            QueryStrategy queryStrategy = new QueryStrategy();
            foreach (DataRow row in _mainTable.Rows) {
                queryStrategy.SetQuery(new InsertQuery() {
                    Query = $"update Resident set FIRST_NAME='{row.ItemArray[0]}', LAST_NAME='{row.ItemArray[1]}', PUROK='{row.ItemArray[2]}', ADDRESS='{row.ItemArray[3]}', CONTACT_NUMBER='{row.ItemArray[4]}', EMERGENCY_CONTACT_NUMBER='{row.ItemArray[5]}', EMERGENCY_CONTACT_NAME='{row.ItemArray[6]}' where RESIDENT_KEY in (select RESIDENT_KEY from Resident where FIRST_NAME='{row.ItemArray[0]}' and LAST_NAME='{row.ItemArray[1]}' and PUROK='{row.ItemArray[2]}' and ADDRESS='{row.ItemArray[3]}')"
                });
            }

            _mainTable.Clear();
            _mainTable = queryStrategy.MainDataTable;

            LoadResidentList(string.Empty);
        }
    }
}