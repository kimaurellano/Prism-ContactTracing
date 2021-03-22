using Microsoft.Data.Sqlite;
using PrismContactTracing.Core.Interface;
using System.Data;
using System.Windows;

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
            string tableName = string.Empty;
            bool capture = false;
            foreach (var str in Query.Split(' ')) {
                if (capture) {
                    tableName = str;

                    break;
                }

                if (str.ToLower().Equals("from")) {
                    capture = true;
                }
            }

            _dbConnector.DbConnectionInstance.Open();

            SqliteCommand getCountCommand = new SqliteCommand($"select count(*) from {tableName};", _dbConnector.DbConnectionInstance);

            // It might be an issue if there's a VERY LARGE data involved.
            int rowCount = 0;
            using (var reader = getCountCommand.ExecuteReader()) {
                while (reader.Read()) {
                    rowCount = int.Parse(reader.GetString(0));
                }
            }

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
                    for (int i = 0; i < rowCount; i++) {
                        var currentRow = _dataTable.NewRow();
                        for (int j = 0; j < columCount; j++) {
                            currentRow[j] = reader.GetString(j);
                        }

                        _dataTable.Rows.Add(currentRow);
                    }
                }
            }

            _dbConnector.Disconnect();

            return _dataTable;
        }
    }
}
