namespace PrismContactTracing.Core.Listener {
    public interface IDataListener {
        string Procedure { get; set; }
        public void StartListen();
    }
}