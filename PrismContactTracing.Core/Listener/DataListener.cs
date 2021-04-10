using MySql.Data.MySqlClient;
using PrismContactTracing.Core.DataComponent;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Windows;

namespace PrismContactTracing.Core.Listener {
    public class DataListener : IDataListener {
        private BackgroundWorker _backgroundWorker;
        private DbConnector _dbConnector;

        /// <summary>
        /// Set table to listen changes from
        /// </summary>
        public string Procedure { get; set; }

        public static event OnTableChange OnTableChangeEvent;

        public delegate void OnTableChange();

        public DataListener() {
            _backgroundWorker = new BackgroundWorker() {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
        }

        public void StartListen() {
            _backgroundWorker.DoWork += OnDoWork;
            _backgroundWorker.RunWorkerCompleted += OnRunWorkerCompleted;
            _backgroundWorker.ProgressChanged += OnProgressChanged;

            // Start backgroundworker
            _backgroundWorker.RunWorkerAsync();
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

                Thread.Sleep(10000);
            }
        }

        void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // This will run on the main form thread when the background work is
            // done; it connects the results to the data grid.
            _dbConnector.Disconnect();
        }
    }
}
