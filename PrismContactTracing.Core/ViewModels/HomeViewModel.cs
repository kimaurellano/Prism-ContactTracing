using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Interface;
using PrismContactTracing.Core.Models;
using System.Collections.ObjectModel;
using System.Data;

namespace PrismContactTracing.Core.ViewModels {
    public class HomeViewModel : BindableBase {

        private IRegionManager _regionManager;
        private IDbConnector _dbConnector;
        private string _currentGridContent;
        private ObservableCollection<ResidentModel> _residents = new ObservableCollection<ResidentModel>();
        private DataTable _mainTable;
        private ResidentModel _resident;

        public DelegateCommand AddNewResidentCommand { get; private set; }

        public DataTable MainDataTable {
            get => _mainTable;
            set { SetProperty(ref _mainTable, value); }
        }

        // <DataGrid ItemsSource="{Binding EmployeeDataTable}"/>

        public ObservableCollection<ResidentModel> Residents {
            get => _residents;
            set { SetProperty(ref _residents, value); }
        }

        public ResidentModel Resident {
            get => _resident;
            set { 
                SetProperty(ref _resident, value);
                RaisePropertyChanged("");
            }
        }

        public string CurrentGridContent {
            get => _currentGridContent;
            set { SetProperty(ref _currentGridContent, value); }
        }

        public HomeViewModel(IRegionManager regionManager, IDbConnector dbConnector) {
            _regionManager = regionManager;
            _dbConnector = dbConnector;

            LoadDataTable();
        }

        private void LoadDataTable() {
            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new SelectQuery() { 
                Query = "select FIRST_NAME, LAST_NAME, PUROK, CONTACT_NUMBER, ADDRESS, EMERGENCY_CONTACT_NUMBER, EMERGENCY_CONTACT_NAME from Resident"
            });
            _mainTable = queryStrategy.MainDataTable;
        }
    }
}
