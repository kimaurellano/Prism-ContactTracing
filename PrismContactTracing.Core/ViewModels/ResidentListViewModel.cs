using MySql.Data.MySqlClient;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Interface;
using PrismContactTracing.Core.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrismContactTracing.Core.ViewModels {
    public class ResidentListViewModel : BindableBase {
        private IRegionManager _regionManager;
        private DataTable _mainTable;
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
        private bool _isDialogOpen = false;
        private bool _isEnableEdit = false;
        private bool _isSavingEnable = false;
        private bool _onResidentReportLoaded = true; // To identify which table is displayed then apply proper filter through search input
        private Visibility _isVisible;

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
        public DelegateCommand ExecuteSearchContentCommand { get; private set; }
        public DelegateCommand<object> ExecuteLoadResidentsReportCommand { get; private set; }
        public DelegateCommand ExecuteGenericDelegateOpenDialogCommand { get; private set; }
        public DelegateCommand ExecuteIsEnableEditCommand { get; private set; }
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

        public bool IsDialogOpen {
            get => _isDialogOpen;
            set { SetProperty(ref _isDialogOpen, value); }
        }

        public Visibility IsVisible {
            get => _isVisible;
            set { SetProperty(ref _isVisible, value); }
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

        public ResidentListViewModel() {
            //Start read serial data

            Task.Run(() => LoadResidentList(string.Empty));

            ExecuteSearchContentCommand = new DelegateCommand(LoadContentBySearch);
            ExecuteLoadResidentsReportCommand = new DelegateCommand<object>(async (p) => await LoadResidentList(string.Empty));
            ExecuteGenericDelegateOpenDialogCommand = new DelegateCommand(() => { IsDialogOpen = !IsDialogOpen; });
        }

        private void LoadContentBySearch() {
            if (_onResidentReportLoaded) {
                Task.Run(() => LoadResidentList(_residentName));
            } else {
                //
            }
        }

        /// <param name="resident">Empty value will get all rows else otherwise</param>
        private async Task LoadResidentList(string resident) {
            await Task.Run(() => {
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
        }
    }
}