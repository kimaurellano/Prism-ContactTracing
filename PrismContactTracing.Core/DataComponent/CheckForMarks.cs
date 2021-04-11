using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.Collections.Generic;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {

    /// <summary>
    /// This is for checking DataTable with "Mark for" columns.
    /// Just do something on the data with 'Marked = true'.
    /// This is made for that purpose obviously duh
    /// </summary>
    public class CheckForMarks : IQueryStrategy {

        private DbConnector _dbConnector;

        public string ParameterName { get; set; }
        public string Procedure { get; set; }
        public List<string> ParameterValues { get; set; }
        /// <summary>
        /// DataTable SHOULD HAVE "Marked for" column
        /// </summary>
        public DataTable TargetDataTable { get; set; }

        public CheckForMarks() {
            _dbConnector = new DbConnector();
        }

        public DataTable DoQuery() {
            _dbConnector.Connect();

            
            List<string> keys = new List<string>();
            if(TargetDataTable != null) {
                // When target table is supplied for checking
                for (int i = 0; i < TargetDataTable.Rows.Count; i++) {
                    // Rows marked for delete
                    if ((bool)TargetDataTable.Rows[i].ItemArray[TargetDataTable.Columns.Count - 1]) {
                        keys.Add(TargetDataTable.Rows[i][TargetDataTable.Columns[0].ColumnName].ToString());
                    }
                }
            } else {
                // Values are manually supplied and will change accordingly
                foreach (var value in ParameterValues) {
                    keys.Add(value);
                }
            }

            // Do query for how many rows are changed   
            foreach (string key in keys) {
                MySqlCommand command = new MySqlCommand(Procedure, _dbConnector.DbConnectionInstance) {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue(ParameterName, key);

                command.ExecuteNonQuery();
                command.Dispose();
            }

            return null;
        }
    }
}
