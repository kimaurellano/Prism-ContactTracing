using MySql.Data.MySqlClient;
using PrismContactTracing.Core.Interface;
using System.Collections.Generic;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class GetDataQuery : IQueryStrategy {

        private DataTable _dataTable;
        private DbConnector _dbConnector;

        public string Procedure { get; set; }

        public List<KeyValuePair<string, string>> Parameters { get; set; }

        public GetDataQuery() {
            _dbConnector = new DbConnector();
        }

        // Check account existence
        // Get all users
        public DataTable DoQuery() {
            _dbConnector.Connect();

            DataSet dataset = new DataSet("MainDataset");
            _dataTable = new DataTable();
            _dataTable = dataset.Tables.Add("MainTable");

            MySqlCommand command = new MySqlCommand(Procedure, _dbConnector.DbConnectionInstance) { 
                CommandType = CommandType.StoredProcedure
            };

            if (Parameters?.Count > 0) {
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

                if (Procedure == "GetResident" || Procedure == "GetResidentsList") {
                    _dataTable.Columns.Add("Mark for archive", typeof(bool));
                    _dataTable.Columns["Mark for archive"].DefaultValue = false;
                } else if (Procedure.Contains("GetArchive")) {
                    _dataTable.Columns.Add("Mark for restore", typeof(bool));
                    _dataTable.Columns["Mark for restore"].DefaultValue = false;
                } else if(Procedure == "GetAdmins") {
                    _dataTable.Columns.Add("Mark for delete", typeof(bool));
                    _dataTable.Columns["Mark for delete"].DefaultValue = false;
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
