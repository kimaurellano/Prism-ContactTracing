using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace PrismContactTracing.Core.DataComponent {
    public class DbConnector : IDbConnector {

        public MySqlConnection DbConnectionInstance { get; set; }

        private string _connection;

        public DbConnector() {
            string res = string.Empty;
            foreach (var file in GetType().Assembly.GetManifestResourceNames()) {
                if (file.Contains("Conf.xml")) {
                    res = file;
                }
            }

            var xDoc = new XDocument();
            using (Stream resourceStream = GetType().Assembly.GetManifestResourceStream(res)) {
                xDoc = XDocument.Load(resourceStream);
            }

            DbConnectionInstance = new MySqlConnection();

            var serverConfig = xDoc.Root;
            string server = serverConfig.Element("server").Value;
            string port = serverConfig.Element("port").Value;
            string uid = serverConfig.Element("uid").Value;
            string password = serverConfig.Element("pwd").Value;
            string database = serverConfig.Element("database").Value;
            _connection = $"server={server};port={port};uid={uid};pwd={password};database={database}";
        }

        public bool Connect() {
            bool isConnected = false;
            try {
                DbConnectionInstance.ConnectionString = _connection;
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
