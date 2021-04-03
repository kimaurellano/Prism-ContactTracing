using Microsoft.Data.Sqlite;
using PrismContactTracing.Core.Interface;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class SelectQuery : IQueryStrategy {

        private DataTable _dataTable;
        private DbConnector _dbConnector;

        public string Query { get; set; }

        public SelectQuery() {
            _dbConnector = new DbConnector();
        }

        // Check account existence
        // Get all users
        public DataTable DoQuery() {
            try {
                _dbConnector.DbConnectionInstance.Open();

                DataSet _ds = new DataSet("MainDataset");
                _dataTable = new DataTable();
                _dataTable = _ds.Tables.Add("MainTable");

                SqliteCommand command = new SqliteCommand(Query + ";", _dbConnector.DbConnectionInstance);
                using (var reader = command.ExecuteReader()) {
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
            } catch (System.Exception) {
                throw;
            }
        }
    }
}
