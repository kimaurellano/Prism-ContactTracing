using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PrismContactTracing.Core.DataComponent {
    public class UpdateQuery : IQueryStrategy {

        private DbConnector _dbConnector;

        public string Procedure { get; set; }
        public DataTable TargetDataTable { get; set; }

        public UpdateQuery() {
            _dbConnector = new DbConnector();
        }

        public DataTable DoQuery() {
            _dbConnector.Connect();

            // Populates DataSet with records from the server
            MySqlDataAdapter adapter = new MySqlDataAdapter() {
                SelectCommand = new MySqlCommand(Procedure, _dbConnector.DbConnectionInstance) { 
                    CommandType = CommandType.StoredProcedure
                }
            };

            DataTable serverTable = new DataTable("TempTable");
            adapter.Fill(serverTable);

            // Do updates
            serverTable = CompareTable(serverTable, TargetDataTable);

            MySqlCommandBuilder builder = new MySqlCommandBuilder(adapter);
            builder.GetUpdateCommand();

            adapter.Update(serverTable);

            _dbConnector.Disconnect();

            return null;
        }

        /// <summary>
        /// Identify changes made in target then apply to source
        /// </summary>
        /// <param name="source">Server sourced table</param>
        /// <param name="target">Datagrid sourced table</param>
        private DataTable CompareTable(DataTable source, DataTable target) {
            for (int i = 0; i < target.Rows.Count; i++) {
                for (int j = 0; j < target.Columns.Count; j++) {
                    string sourceData = source.Rows[i].ItemArray[j].ToString();
                    string targetData = target.Rows[i].ItemArray[j].ToString();
                    if(sourceData != targetData) {
                        source.Rows[i][source.Columns[j].ColumnName] = targetData;
                    }
                }
            }

            return source;
        }
    }
}
