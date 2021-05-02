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

        public enum Mark {
            Archive = 2,
            QR = 1
        }

        private DbConnector _dbConnector;

        public string ParameterName { get; set; }
        public string Procedure { get; set; }
        public List<string> ParameterValues { get; set; }
        /// <summary>
        /// DataTable SHOULD HAVE "Marked for" column
        /// </summary>
        public DataTable TargetDataTable { get; set; }
        /// <summary>
        /// Which (column.count - x) position will be checked
        /// </summary>
        public Mark MarkType { get; set; }
        public List<string> RowDatas { get; private set; }

        public CheckForMarks() {
            _dbConnector = new DbConnector();
            RowDatas = new List<string>();
        }

        public DataTable DoQuery() {
            _dbConnector.Connect();

            List<string> keys = new List<string>();
            if(TargetDataTable != null) {
                // When target table is supplied for checking
                for (int i = 0; i < TargetDataTable.Rows.Count; i++) {
                    // Check marked rows and of which mark type
                    if ((bool)TargetDataTable.Rows[i].ItemArray[TargetDataTable.Columns.Count - (int)MarkType]) {
                        keys.Add(TargetDataTable.Rows[i][TargetDataTable.Columns[0].ColumnName].ToString());
                        
                        if(MarkType == Mark.QR) {
                            string residentQRInfo = string.Empty;
                            for (int j = 1; j <= 7; j++) {
                                residentQRInfo += TargetDataTable.Rows[i].ItemArray[j].ToString() + ",";
                            }

                            RowDatas.Add(residentQRInfo.Remove(residentQRInfo.Length - 1));
                        }
                    }
                }
            } else {
                // Values are manually supplied and will change accordingly
                foreach (var value in ParameterValues) {
                    keys.Add(value);
                }
            }

            // Procedure not supplied means TargetDataTable is the only needed.
            // No need to query from server
            if (Procedure == string.Empty || Procedure == null) {
                return null;
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
