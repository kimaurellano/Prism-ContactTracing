namespace PrismContactTracing.Core.Models {
    public class ResidentContactTraceModel {
        public int ResidentContactTraceKey { get; set; }
        public int ResidentKey { get; set; }
        public string TimeIn { get; set; }
        public string Temperature { get; set; }
        public string HasCoughs { get; set; }
        public string HasColds { get; set; }
        public string HasFever { get; set; }
    }
}
