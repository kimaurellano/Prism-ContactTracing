using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PrismContactTracing.Core.DataComponent {
    public class DbConnector : IDbConnector {

        public SqliteConnection DbConnectionInstance { get; set; }

        private string _connection;

        public DbConnector() {
            DbConnectionInstance = new SqliteConnection($"Data Source={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Resource/ContactTracing.db;");
        }

        public bool Connect() {
            bool isConnected = false;
            try {
                DbConnectionInstance.Open();
            } catch (MySqlException e) {
                MessageBox.Show(e.Message);
                return isConnected;
            }

            return !isConnected;
        }

        public void Disconnect() {
            DbConnectionInstance.Close();
        }
    }
}
