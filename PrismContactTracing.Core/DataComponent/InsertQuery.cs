using Microsoft.Data.Sqlite;
using PrismContactTracing.Core.Interface;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class InsertQuery : IQueryStrategy {

        private DataTable _dataTable;
        private DbConnector _dbConnector;

        public string Query { get; set; }

        public InsertQuery() {
            _dbConnector = new DbConnector();
        }

        public DataTable DoQuery() {
            _dbConnector.DbConnectionInstance.Open();

            DataSet _ds = new DataSet("MainDataset");
            _dataTable = new DataTable();
            _dataTable = _ds.Tables.Add("MainTable");

            SqliteCommand command = new SqliteCommand(Query + ";", _dbConnector.DbConnectionInstance);
            command.ExecuteNonQuery();
            command.Dispose();

            _dbConnector.Disconnect();

            return _dataTable;
        }
    }
}
