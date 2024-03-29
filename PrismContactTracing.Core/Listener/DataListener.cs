﻿using PrismContactTracing.Core.DataComponent;
using System.ComponentModel;
using System.Threading;

namespace PrismContactTracing.Core.Listener {
    public class DataListener : IDataListener {
        private BackgroundWorker _backgroundWorkerForDbListener;
        private BackgroundWorker _backgroundWorkerForSerialListener;
        private DbConnector _dbConnector;

        public static event OnTableChange OnTableChangeEvent;
        public static event OnSerialRead OnSerialReadEvent;

        public delegate void OnTableChange();
        public delegate void OnSerialRead();

        public DataListener() {
            _backgroundWorkerForDbListener = new BackgroundWorker() {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _backgroundWorkerForSerialListener = new BackgroundWorker() {
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

        public void StartWaitForTimeOutComPort() {
            _backgroundWorkerForSerialListener.DoWork += OnDoWorkSerialListen;
            _backgroundWorkerForSerialListener.ProgressChanged += OnDoWorkSerialListenChanged;

            // Start backgroundworker
            _backgroundWorkerForSerialListener.RunWorkerAsync();
        }

        public void CancelWaitForTimeOutComPort() {
            _backgroundWorkerForSerialListener.CancelAsync();
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e) {
            OnTableChangeEvent?.Invoke();
        }

        private void OnDoWorkSerialListenChanged(object sender, ProgressChangedEventArgs e) {
            OnSerialReadEvent?.Invoke();

            _backgroundWorkerForSerialListener.CancelAsync();
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
                // Wait time for arduino response after connection
                Thread.Sleep(10000);

                // Just cause an invoke
                worker.ReportProgress(0, null);

                // No need to run background worker
                worker.CancelAsync();
            }

            e.Cancel = true;
        }

        private void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // This will run on the main form thread when the background work is
            // done; it connects the results to the data grid.
            _dbConnector.Disconnect();
        }
    }
}
