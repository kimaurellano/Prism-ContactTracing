using PrismContactTracing.Core.DataComponent;
using PrismContactTracing.Core.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.IO.Ports;

namespace PrismContactTracing.Core.Listener {
    public class DataListener : IDataListener {
        private BackgroundWorker _backgroundWorkerForDbListener;
        private BackgroundWorker _backgroundWorkerForSerialListener;
        private DbConnector _dbConnector;
        private ResidentContactTraceModel _residentContactTraceModel;
        private SerialPort _serialPort;

        public static event OnTableChange OnTableChangeEvent;
        public static event OnSerialRead OnSerialReadEvent;

        public delegate void OnTableChange();
        public delegate void OnSerialRead(ResidentContactTraceModel residentContactTraceInfo);

        public DataListener() {
            _backgroundWorkerForDbListener = new BackgroundWorker() {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
        }

        public void StartDbChangesListener() {
            _backgroundWorkerForDbListener.DoWork += OnDoWork;
            _backgroundWorkerForDbListener.RunWorkerCompleted += OnRunWorkerCompleted;
            _backgroundWorkerForDbListener.ProgressChanged += OnProgressChanged;

            // Start backgroundworker
            _backgroundWorkerForDbListener.RunWorkerAsync();
        }

        public void StartSerialReadListener(SerialPort configs) {
            SerialPort serialPort = configs;
            if (!serialPort.IsOpen) {
                serialPort.Open();
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e) {
            OnTableChangeEvent?.Invoke();
        }

        private void OnDoWork(object sender, DoWorkEventArgs e) {
            _dbConnector = new DbConnector();
            _dbConnector.Connect();

            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending) {
                // Just cause an invoke
                worker.ReportProgress(0, null);

                Thread.Sleep(120000);
            }
        }

        private void OnDoWorkSerialListen(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending) {
                // Just cause an invoke
                worker.ReportProgress(0, null);
            }
        }

        void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // This will run on the main form thread when the background work is
            // done; it connects the results to the data grid.
            _dbConnector.Disconnect();
        }
    }
}
