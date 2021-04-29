﻿using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Listener;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PrismContactTracing.Core.ViewModels {
    public class HomeViewModel : BindableBase {

        private IRegionManager _regionManager;
        private ObservableCollection<string> _portList;
        private SerialPort _serialPort;
        private string _realTimeLog;
        private string _realTimeDateLog;
        private string _currentItem;
        private bool _showConfirmDialog;
        private bool _serialButtonEnable;
        private Visibility _logVisibility;
        private string _serialLog;

        public DelegateCommand<string> ConnectPortCommand { get; private set; }
        public DelegateCommand<string> NavigateToCommand { get; private set; }
        public DelegateCommand ExecuteLogoutCommand { get; private set; }
        public DelegateCommand ExecuteConfirmCommand { get; private set; }
        public string ConnectionState { get; private set; }
        public string ResidentId { get; private set; }
        public string Temperature { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string HasCoughs { get; private set; }
        public string HasColds { get; private set; }
        public string HasFever { get; private set; }
        public string TimeIn { get; private set; }
        
        public string SerialLog {
            get => _serialLog; 
            set {
                SetProperty(ref _serialLog, value);
            } 
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
            set { 
                SetProperty(ref _portList, value);
            }
        }

        public Visibility LogVisibility {
            get => _logVisibility;
            set {
                SetProperty(ref _logVisibility, value); 
            }
        }

        public bool SerialButtonEnable {
            get => _serialButtonEnable;
            set {
                SetProperty(ref _serialButtonEnable, value);
            }
        }

        public string RealTimeDateLog {
            get => _realTimeDateLog;
            set { SetProperty(ref _realTimeDateLog, value); }
        }

        public string RealTimeLog {
            get => _realTimeLog;
            set { SetProperty(ref _realTimeLog, value); } 
        }

        public bool ShowConfirmDialog {
            get => _showConfirmDialog;
            set { SetProperty(ref _showConfirmDialog, value); }
        }

        public HomeViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;

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
        }

        private void StartListen(string port) {
            if (!_serialPort.IsOpen && _serialPort.PortName != port) {
                // Just in case other port are opened before
                _serialPort.Close();

                _serialPort.PortName = port;
                _serialPort.BaudRate = 115200;
                _serialPort.Open();
                _serialPort.DtrEnable = true;
                _serialPort.DataReceived += RefreshTraceLog;

                SerialButtonEnable = !SerialButtonEnable;
                RaisePropertyChanged("SerialButtonEnable");
            } else {
                _serialPort.Close();
                _serialPort.DataReceived -= RefreshTraceLog;
                _serialPort.PortName = "COMX";

                SerialLog = "Disconnected";
                RaisePropertyChanged("SerialLog");
            }

            LogVisibility = _serialPort.IsOpen ? Visibility.Visible : Visibility.Hidden;
            RaisePropertyChanged("LogVisibility");

            ConnectionState = _serialPort.IsOpen ? "Disconnect" : "Connect";
            RaisePropertyChanged("ConnectionState");
        }

        private void RefreshTraceLog(object sender, SerialDataReceivedEventArgs e) {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();

            if (indata.ToLower().Contains("connected")) {
                SerialLog = "Connected";
                RaisePropertyChanged("SerialLog");

                SerialButtonEnable = !SerialButtonEnable;
                RaisePropertyChanged("SerialButtonEnable");
            }

            //string value = indata.Split(":")[1];
            //if (indata.Contains("KEY:")) {
            //    ResidentId = value;
            //} else if (indata.Contains("TEMP:")) {
            //    Temperature = value;
            //} else if (indata.Contains("HASCOUGH:")) {
            //    HasCoughs = value;
            //} else if (indata.Contains("HASCOLDS:")) {
            //    HasColds = value;
            //} else if (indata.Contains("HASFEVER:")) {
            //    HasFever = value;

            //    // We expect that the HasFever will always be the last to checked
            //    Task.Run(() => InsertResident());
            //}
        }

        private void NavigateTo(string page) {
            if (page != null) {
                _regionManager.RequestNavigate("ContentType", page);
            }
        }

        private void TimerTick(object sender, EventArgs e) {
            string timeInfo = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
            _realTimeLog = timeInfo.Split(' ')[1] + timeInfo.Split(' ')[2];
            RaisePropertyChanged("RealTimeLog");
            _realTimeDateLog = timeInfo.Split(' ')[0];
            RaisePropertyChanged("RealTimeDateLog");
        }

        private async Task InsertResident() {
            await Task.Run(() => {
                List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>();
                parameter.Add(new KeyValuePair<string, string>("@m_resident_key", ResidentId));
                parameter.Add(new KeyValuePair<string, string>("@m_time_in", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")));
                parameter.Add(new KeyValuePair<string, string>("@m_temperature", Temperature));
                parameter.Add(new KeyValuePair<string, string>("@m_has_coughs", HasCoughs));
                parameter.Add(new KeyValuePair<string, string>("@m_has_colds", HasColds));
                parameter.Add(new KeyValuePair<string, string>("@m_has_fever", HasFever));

                QueryStrategy queryStrategy = new QueryStrategy();
                queryStrategy.SetQuery(new InsertQuery() {
                    Procedure = "InsertContactTraceRecord",
                    Parameters = parameter
                });
            });
        }
    }
}