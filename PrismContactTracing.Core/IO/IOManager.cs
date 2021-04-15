using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using ClosedXML.Excel;
using DataTable = System.Data.DataTable;

namespace PrismContactTracing.Core.IO {
    public class IOManager {
        public bool IsSaved { get; private set; }

        /// <param name="sourceTable">Datatable that will be converted as excel</param>
        public void ExportToExcel(DataTable sourceTable, string sheetName) {
            SaveFileDialog saveFileDialog = new SaveFileDialog {
                Filter = "Excel file|*.xlsx",
                Title = "Save excel file"
            };
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == string.Empty) {
                return;
            }

            XLWorkbook wb = new XLWorkbook();
            wb.Worksheets.Add(sourceTable, sheetName);
            wb.SaveAs($"{saveFileDialog.FileName}");

            IsSaved = File.Exists(saveFileDialog.FileName);
        }
    }
}
