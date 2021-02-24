using MySql.Data.MySqlClient;

namespace PrismContactTracing.Core.Interface {
    public interface IDbConnector {
        public MySqlConnection DbConnectionInstance { get; set; }
        public bool Connect();
        public void Disconnect();
    }
}
