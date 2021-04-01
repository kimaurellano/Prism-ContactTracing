using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Interface;
using PrismContactTracing.Core.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace PrismContactTracing.Core.ViewModels {
    public class HomeViewModel : BindableBase {

        private IRegionManager _regionManager;
        private ObservableCollection<ResidentModel> _residents = new ObservableCollection<ResidentModel>();
        private DataTable _mainTable;
        private DataRowView _residentDataRowView;
        private string _residentName;
        private string _searchType;
        private Visibility _isVisible;
        private Visibility _backVisibility;

        public DelegateCommand AddNewResidentCommand { get; private set; }

        public DelegateCommand ExecuteGenericDelegateGetResidentsCommand { get; private set; }

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
                LoadResident(_residentName);
                RaisePropertyChanged("MainDataTable");
            }
        }

        public string SearchType {
            get => _searchType;
            set {
                SetProperty(ref _searchType, value);
            }
        }

        public Visibility BackVisibility {
            get => _backVisibility;
            set { SetProperty(ref _backVisibility, value); }
        }

        public Visibility IsVisible {
            get => _isVisible;
            set { SetProperty(ref _isVisible, value); }
        }

        public DataRowView ResidentDataRowView {
            get => _residentDataRowView;
            set {
                if (value != null) {
                    SetProperty(ref _residentDataRowView, value);
                    LoadCloseContactTrace(_residentDataRowView.Row["First Name"].ToString(), _residentDataRowView.Row["Last Name"].ToString(), _residentDataRowView.Row["Time In"].ToString());
                    RaisePropertyChanged("MainDataTable");
                    IsVisible = Visibility.Hidden;
                    BackVisibility = Visibility.Visible;
                }
            }
        }

        public HomeViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;

            //Start read serial data

            LoadDataTable();

            ExecuteGenericDelegateGetResidentsCommand = new DelegateCommand(LoadDataTable);
        }

        private void LoadDataTable() {
            IsVisible = Visibility.Visible;
            BackVisibility = Visibility.Hidden;

            SearchType = $"List of residents";

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() { 
                Query = "select r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', rct.TIME_IN as 'Time In' from Resident r left join ResidentContactTrace rct on r.RESIDENT_KEY = rct.RESIDENT_KEY"
            });

            // Avoid clearing on load
            if(_mainTable != null) {
                _mainTable.Clear();
            }

            _mainTable = queryStrategy.MainDataTable;

            // Avoiding the main table not to update
            RaisePropertyChanged("MainDataTable");
        }

        private void LoadResident(string name) {
            SearchType = $"List of residents";

            if (name == string.Empty) {
                LoadDataTable();
                
                return;
            }

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() {
                Query = $"select r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', rct.TIME_IN as 'Time In' from Resident r left join ResidentContactTrace rct on r.RESIDENT_KEY = rct.RESIDENT_KEY where r.FIRST_NAME LIKE '%{name}%'"
            });

            _mainTable.Clear();
            _mainTable = queryStrategy.MainDataTable;
        }

        private void LoadCloseContactTrace(string firstName, string lastName, string timeIn) {
            SearchType = $"Close contact of {firstName} {lastName} @ {timeIn}";

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() {
                Query = $"select r.FIRST_NAME as 'First Name', r.LAST_NAME as 'Last Name', r.PUROK as 'Purok', rct.TIME_IN as 'Time In' from Resident r left join ResidentContactTrace rct on r.RESIDENT_KEY = rct.RESIDENT_KEY where rct.TIME_IN LIKE '%{timeIn}%';"
            });

            _mainTable.Clear();
            _mainTable = queryStrategy.MainDataTable;
        }
    }
}
