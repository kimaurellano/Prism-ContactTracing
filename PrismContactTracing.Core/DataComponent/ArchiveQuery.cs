using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.Collections.Generic;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class ArchiveQuery : IQueryStrategy {

        private DbConnector _dbConnector;

        public string Procedure { get; set; }

        public List<KeyValuePair<string, string>> Parameters { get; set; }

        public ArchiveQuery() {
            _dbConnector = new DbConnector();
        }

        public DataTable DoQuery() {
            _dbConnector.Connect();

            MySqlCommand command = new MySqlCommand(Procedure, _dbConnector.DbConnectionInstance) {
                CommandType = CommandType.StoredProcedure
            };

            if (Parameters.Count > 0) {
                foreach (var parameter in Parameters) {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            command.ExecuteNonQuery();
            command.Dispose();

            return null;
        }
    }
}
