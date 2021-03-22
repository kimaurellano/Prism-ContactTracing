using Microsoft.Data.Sqlite;

namespace PrismContactTracing.Core.Interface {
    public interface IDbConnector {
        public SqliteConnection DbConnectionInstance { get; set; }
        public bool Connect();
        public void Disconnect();
    }
}
