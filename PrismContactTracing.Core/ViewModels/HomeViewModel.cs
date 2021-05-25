using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Listener;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PrismContactTracing.Core.ViewModels {
    public class HomeViewModel : BindableBase {

        private IRegionManager _regionManager;
        private IDataListener _dataListener;
        private ObservableCollection<string> _portList;
        private SerialPort _serialPort;
        private string _realTimeLog;
        private string _realTimeDateLog;
        private string _currentItem;
        private bool _showConfirmDialog;
        private bool _serialButtonEnable;
        private Visibility _logVisibility;
        private string _serialLog;
        private bool _allowPortChange;
        private string _connectionState;
        private string _hasColds;
        private string _hasCoughs;
        private string _lastName;
        private string _firstName;
        private string _timeIn;
        private string _hasFever;
        private string _temperature;
        private string _residentId;

        public DelegateCommand<string> ConnectPortCommand { get; private set; }
        public DelegateCommand<string> NavigateToCommand { get; private set; }
        public DelegateCommand ExecuteLogoutCommand { get; private set; }
        public DelegateCommand ExecuteConfirmCommand { get; private set; }

        public string ResidentId {
            get => _residentId;
            set { SetProperty(ref _residentId, value); RaisePropertyChanged("ResidentId"); }
        }

        public string Temperature {
            get => _temperature;
            set { SetProperty(ref _temperature, value); RaisePropertyChanged("Temperature"); }
        }

        public string FirstName {
            get => _firstName;
            set { SetProperty(ref _firstName, value); RaisePropertyChanged("FirstName"); }
        }

        public string LastName {
            get => _lastName;
            set { SetProperty(ref _lastName, value); RaisePropertyChanged("LastName"); }
        }

        public string HasCoughs {
            get => _hasCoughs;
            set { SetProperty(ref _hasCoughs, value); RaisePropertyChanged("HasCoughs"); }
        }

        public string HasColds { 
            get => _hasColds; 
            set { SetProperty(ref _hasColds, value); RaisePropertyChanged("HasColds"); } 
        }

        public string HasFever {
            get => _hasFever;
            set { SetProperty(ref _hasFever, value); RaisePropertyChanged("HasFever"); }
        }

        public string TimeIn {
            get => _timeIn;
            set { SetProperty(ref _timeIn, value); RaisePropertyChanged("TimeIn"); }
        }

        public string ConnectionState { 
            get => _connectionState; 
            set { SetProperty(ref _connectionState, value); RaisePropertyChanged("ConnectionState"); } 
        }

        public string SerialLog {
            get => _serialLog; 
            set { SetProperty(ref _serialLog, value); RaisePropertyChanged("SerialLog"); } 
        }

        public string CurrentItem { 
            get => _currentItem;
            set {
                SetProperty(ref _currentItem, value);
                _serialButtonEnable = _currentItem != string.Empty;
                RaisePropertyChanged("SerialButtonEnable");
            }
        }

        public ObservableCollection<string> PortList { 
            get => _portList;
            set {  SetProperty(ref _portList, value); }
        }

        public Visibility LogVisibility {
            get => _logVisibility;
            set { SetProperty(ref _logVisibility, value); RaisePropertyChanged("LogVisibility"); }
        }

        public bool AllowPortChange {
            get => _allowPortChange;
            set { SetProperty(ref _allowPortChange, value); RaisePropertyChanged("AllowPortChange"); }
        }

        public bool SerialButtonEnable {
            get => _serialButtonEnable;
            set { SetProperty(ref _serialButtonEnable, value); RaisePropertyChanged("SerialButtonEnable"); }
        }

        public string RealTimeDateLog {
            get => _realTimeDateLog;
            set { SetProperty(ref _realTimeDateLog, value); RaisePropertyChanged("RealTimeDateLog"); }
        }

        public string RealTimeLog {
            get => _realTimeLog;
            set { SetProperty(ref _realTimeLog, value); RaisePropertyChanged("RealTimeLog"); } 
        }

        public bool ShowConfirmDialog {
            get => _showConfirmDialog;
            set { SetProperty(ref _showConfirmDialog, value); }
        }

        public HomeViewModel(IRegionManager regionManager, IDataListener dataListener) {
            _regionManager = regionManager;
            _dataListener = dataListener;

            ConnectPortCommand = new DelegateCommand<string>((port) => StartListen(port));
            NavigateToCommand = new DelegateCommand<string>(NavigateTo);
            ExecuteLogoutCommand = new DelegateCommand(() => { ShowConfirmDialog = !ShowConfirmDialog; });
            ExecuteConfirmCommand = new DelegateCommand(() => { ShowConfirmDialog = !ShowConfirmDialog; Application.Current.Shutdown(); });

            NavigateTo("AdminView");

            DispatcherTimer LiveTime = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(1)
            };

            LiveTime.Tick += TimerTick;
            LiveTime.Start();

            _portList = new ObservableCollection<string>();
            // Populate ports on load
            foreach (var port in SerialPort.GetPortNames()) {
                _portList.Add(port);
            }

            ConnectionState = "Connect";

            _serialPort = new SerialPort();

            AllowPortChange = true;
        }

        private void StartListen(string port) {
            if (!_serialPort.IsOpen) {
                SerialLog = "Connecting...";

                // Just in case other port are opened before. Need to close that first.
                _serialPort.Close();

                _serialPort.PortName = port;
                _serialPort.BaudRate = 115200;
                _serialPort.Open();
                _serialPort.DtrEnable = true;
                _serialPort.DataReceived += RefreshTraceLog;

                SerialButtonEnable = !SerialButtonEnable;

                _dataListener.StartWaitForTimeOutComPort();
                DataListener.OnSerialReadEvent += ConnectionTimeOut;
            } else {
                _serialPort.Close();
                _serialPort.DataReceived -= RefreshTraceLog;

                SerialLog = "Disconnected";
            }

            LogVisibility = _serialPort.IsOpen ? Visibility.Hidden : Visibility.Visible;
            ConnectionState = _serialPort.IsOpen ? "Disconnect" : "Connect";
            AllowPortChange = !_serialPort.IsOpen;
        }

        private void ConnectionTimeOut() {
            DataListener.OnSerialReadEvent -= ConnectionTimeOut;
            StartListen(_serialPort.PortName);

            SerialButtonEnable = !SerialButtonEnable;

            SerialLog = "Invalid port";
        }

        private void RefreshTraceLog(object sender, SerialDataReceivedEventArgs e) {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            sp.DiscardInBuffer();

            if (indata.ToLower().Contains("start")) {
                SerialLog = "Connected";

                SerialButtonEnable = !SerialButtonEnable;

                // No need to run timeout check since we are already connected
                _dataListener.CancelWaitForTimeOutComPort();
                DataListener.OnSerialReadEvent -= ConnectionTimeOut;
            }

            // We are expecting value like this
            // QR:FirstName,LastName,Purok,Address,ContactNumber,EContact,EName
            if (indata.Contains(",")) {
                string qrData = indata.Trim();
                qrData = qrData.Replace("\0", "");

                // Clear for next qr scan
                ResidentId = string.Empty;
                FirstName = string.Empty;
                LastName = string.Empty;
                HasColds = string.Empty;
                HasCoughs = string.Empty;
                HasFever = string.Empty;
                Temperature = string.Empty;

                DataTable resultTable = CheckResident(qrData);
                if(resultTable == null) {
                    MessageBox.Show($"malformed/not found data: {qrData}", "Record info");
                    return;
                }

                // Only one row is populated to the datatable. so idx 0(row 0 at col 0) we use.
                ResidentId = resultTable.Rows[0].ItemArray[0].ToString();
                RaisePropertyChanged("ResidentId");

                if(resultTable.Rows.Count > 0) {
                    // Allow body scan
                    sp.Write("Registered");
                } else {
                    sp.Write("Not Registered");
                }
            }

            if (indata.Contains("TEMP:")) {
                Temperature = indata.Split(":")[1];
                HasFever = float.Parse(Temperature) > 37.2 ? "YES" : "NO";
            } else if (indata.Contains("COLD:")) {
                HasColds = indata.Split(":")[1];

                // I did not find any fever check only last one checked is cough.
                // Then shoud insert after cough check.
                Task.Run(() => InsertResidentContactTrace());
            } else if (indata.Contains("COUGH:")) {
                HasCoughs = indata.Split(":")[1];
            } else if (indata.Contains("FEVER:")) {
                HasFever = indata.Split(":")[1];
            }
        }

        private void NavigateTo(string page) {
            if (page != null) {
                _regionManager.RequestNavigate("ContentType", page);
            }
        }

        private void TimerTick(object sender, EventArgs e) {
            string timeInfo = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
            RealTimeLog = timeInfo.Split(' ')[1] + timeInfo.Split(' ')[2];
            RealTimeDateLog = timeInfo.Split(' ')[0];
        }

        private DataTable CheckResident(string qrData) {
            List<string> residentInfo = new List<string>();
            foreach (var data in qrData.Split(',')) {
                residentInfo.Add(data);
            }

            if(residentInfo.Count < 7) {
                return null;
            }

            FirstName = residentInfo[0];
            LastName = residentInfo[1];

            List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("@m_firstname", residentInfo[0]),
                new KeyValuePair<string, string>("@m_lastname", residentInfo[1]),
                new KeyValuePair<string, string>("@m_purok", residentInfo[2]),
                new KeyValuePair<string, string>("@m_address", residentInfo[3]),
                new KeyValuePair<string, string>("@m_contact", residentInfo[4]),
                new KeyValuePair<string, string>("@m_econtact", residentInfo[5]),
                new KeyValuePair<string, string>("@m_ename", residentInfo[6])
            };

            QueryStrategy queryStrategy = new QueryStrategy();
            queryStrategy.SetQuery(new GetDataQuery() {
                Procedure = "GetUniqueResident",
                Parameters = parameter
            });

            return queryStrategy.MainDataTable;
        }

        private async Task InsertResidentContactTrace() {
            await Task.Run(() => {
                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                parameter.Add(new KeyValuePair<string, string>("@m_resident_key", ResidentId));
                parameter.Add(new KeyValuePair<string, string>("@m_time_in", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")));
                parameter.Add(new KeyValuePair<string, string>("@m_temperature", Temperature));
                parameter.Add(new KeyValuePair<string, string>("@m_has_coughs", HasCoughs));
                parameter.Add(new KeyValuePair<string, string>("@m_has_colds", HasColds));
                parameter.Add(new KeyValuePair<string, string>("@m_has_fever", float.Parse(Temperature) > 37.2 ? "YES" : "NO"));

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new InsertQuery() {
                    Procedure = "InsertContactTraceRecord",
                    Parameters = parameter
                });
            });
        }
    }
}