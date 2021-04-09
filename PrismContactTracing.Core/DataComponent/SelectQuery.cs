using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.Collections.Generic;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class SelectQuery : IQueryStrategy {

        private DataTable _dataTable;
        private DbConnector _dbConnector;

        public string Procedure { get; set; }

        public List<KeyValuePair<string, string>> Parameters { get; set; }

        public SelectQuery() {
            _dbConnector = new DbConnector();
        }

        // Check account existence
        // Get all users
        public DataTable DoQuery() {
            _dbConnector.Connect();

            DataSet _ds = new DataSet("MainDataset");
            _dataTable = new DataTable();
            _dataTable = _ds.Tables.Add("MainTable");

            MySqlCommand command = new MySqlCommand(Procedure, _dbConnector.DbConnectionInstance) { 
                CommandType = CommandType.StoredProcedure
            };

            if (Parameters.Count > 0) {
                foreach (var parameter in Parameters) {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }
            
            using (MySqlDataReader reader = command.ExecuteReader()) {
                if (!reader.HasRows) {
                    return null;
                }

                int columCount = reader.FieldCount;
                for (int i = 0; i < columCount; i++) {
                    _dataTable.Columns.Add(reader.GetName(i));
                }

                while (reader.Read()) {
                    var currentRow = _dataTable.NewRow();
                    for (int i = 0; i < columCount; i++) {
                        if (reader.IsDBNull(i)) {
                            currentRow[i] = string.Empty;
                            continue;
                        }
                        currentRow[i] = reader.GetString(i);
                    }

                    _dataTable.Rows.Add(currentRow);
                }
            }

            _dbConnector.Disconnect();

            return _dataTable;
        }
    }
}
