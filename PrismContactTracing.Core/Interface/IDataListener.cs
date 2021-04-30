namespace PrismContactTracing.Core.Listener {
    public interface IDataListener {
        public void StartDbChangesListener();
        public void StartWaitForTimeOutComPort();
        public void CancelWaitForTimeOutComPort();
    }
}