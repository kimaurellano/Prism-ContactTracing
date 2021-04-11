using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.Collections.Generic;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class ArchiveQuery : IQueryStrategy {

        private DbConnector _dbConnector;

        public string Procedure { get; set; }
        public DataTable TargetDataTable { get; set; }

        public ArchiveQuery() {
            _dbConnector = new DbConnector();
        }

        public DataTable DoQuery() {
            _dbConnector.Connect();

            // Get the difference between the server sourced data and the datagrid sourced data
            List<string> residentKeys = new List<string>();
            for (int i = 0; i < TargetDataTable.Rows.Count; i++) {
                // Rows marked for delete
                if ((bool)TargetDataTable.Rows[i].ItemArray[8]) {
                    residentKeys.Add(TargetDataTable.Rows[i][TargetDataTable.Columns[0].ColumnName].ToString());
                }
            }

            // Do query for how many rows are changed
            foreach (string key in residentKeys) {
                MySqlCommand command = new MySqlCommand(Procedure, _dbConnector.DbConnectionInstance) {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@m_key", key);

                command.ExecuteNonQuery();
                command.Dispose();
            }

            return null;
        }
    }
}
