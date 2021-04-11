using Prism.Commands;
using Prism.Mvvm;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Listener;
using PrismContactTracing.Core.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrismContactTracing.Core.ViewModels {
    public class ResidentReportViewModel : BindableBase {

        private IDataListener _dataListener;
        private ObservableCollection<ResidentModel> _residents = new ObservableCollection<ResidentModel>();
        private DataTable _mainTable;
        private DataRowView _residentDataRowView;
        private Cursor _cursorType;
        private string _residentName;
        private string _searchType;
        private bool _isDialogOpen = false;
        private bool _isEnableEdit = false;
        private bool _isSavingEnable = false;
        private bool _onResidentReportLoaded = true; // To identify which table is displayed then apply proper filter through search input
        private Visibility _isVisible;
        private Visibility _backVisibility = Visibility.Hidden;
        private Visibility _dataModifierEnable;
        private float _spinnerEnable = 0f;

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

        public ObservableCollection<ResidentModel> Residents {
            get => _residents;
            set { SetProperty(ref _residents, value); }
        }

        public string ResidentName {
            get => _residentName;
            set { SetProperty(ref _residentName, value); }
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
        
        public float SpinnerEnable {
            get => _spinnerEnable;
            set { SetProperty(ref _spinnerEnable, value); }
        }

        public DataRowView ResidentDataRowView {
            get => _residentDataRowView;
            set {
                if (value != null) {
                    SetProperty(ref _residentDataRowView, value);
                    if (_onResidentReportLoaded) {
                        Task.Run(() => LoadCloseContactTrace(_residentDataRowView.Row["Time In"].ToString()));
                        IsVisible = Visibility.Hidden;
                        BackVisibility = Visibility.Visible;
                    }
                }
            }
        }

        public ResidentReportViewModel(IDataListener dataListener) {
            //Start read serial data

            _dataListener = dataListener;
            _dataListener.StartDbChangesListener();

            DataListener.OnTableChangeEvent += RefreshTable;

            Task.Run(() => LoadResidentReport(string.Empty));

            ExecuteSearchContentCommand = new DelegateCommand(async () => await LoadResidentReport(_residentName));
            ExecuteLoadResidentsReportCommand = new DelegateCommand<object>(async (p) => await LoadResidentReport(string.Empty));
            ExecuteGenericDelegateOpenDialogCommand = new DelegateCommand(() => { IsDialogOpen = !IsDialogOpen; });
        }

        /// <param name="resident">Empty value will get all rows else otherwise</param>
        private async Task LoadResidentReport(string resident) {
            await Task.Run(() => {
                SpinnerEnable = 1;

                _cursorType = Cursors.Hand;
                RaisePropertyChanged("CursorType");

                OnResidentReportLoaded = true;
                DataModifierEnable = Visibility.Hidden;

                IsVisible = Visibility.Visible;
                BackVisibility = Visibility.Hidden;
                IsReadOnlyDataGrid = true;

                SearchType = $"Reported list of resident's Time-ins";

                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                if (resident != string.Empty) {
                    parameter.Add(new KeyValuePair<string, string>("@m_resident", resident));
                }
                
                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new GetDataQuery() {
                    Procedure = resident == string.Empty || resident == null ? "GetResidentsReport" : "GetResidentReport",
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

            SpinnerEnable = 0;
        }

        private async Task LoadCloseContactTrace(string timeIn) {
            await Task.Run(() => {
                SpinnerEnable = 1;

                _cursorType = Cursors.Arrow;
                RaisePropertyChanged("CursorType");

                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                parameter.Add(new KeyValuePair<string, string>("@m_time_in", timeIn));

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new GetDataQuery() {
                    Procedure = timeIn == string.Empty ? "GetCloseContactTraces" : "GetCloseContactTrace",
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

            SpinnerEnable = 0;
        }

        private void RefreshTable() {
            Task.Run(() => LoadResidentReport(string.Empty));
        }
    }
}