using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExcelManager
{
    public class ExcelManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static ExcelManager instance = null;
        private const int MAX_COLUMNS = 254;
        private const int MAX_ROWS = 500;

        private ExcelManager() { }

        public static ExcelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new ExcelManager();
                        }
                    }
                }

                return instance;
            }
        }

        #region Public Methods

        #endregion

        #region Private Methods

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        private string CalacRangeColumns(int index)
        {
            string range = string.Empty;
            try
            {
                int columnnum = MAX_COLUMNS;
                string from = GetExcelColumnName(index * columnnum + 1);
                string to = GetExcelColumnName((index * columnnum) + columnnum);
                range = string.Format("{0}1:{1}{2}", from, to, MAX_ROWS);
            }
            catch (Exception)
            {
                range = string.Empty;
            }
            return range;
        }

        private static DataTable MergeAll(IList<DataTable> tables, String primaryKeyColumn)
        {
            try
            {
                DataTable table = new DataTable("TblUnion");

                if (!tables.Any())
                {
                    throw new ArgumentException("Tables must not be empty", "tables");
                }

                if (primaryKeyColumn != null)
                {
                    foreach (DataTable t in tables)
                    {
                        if (!t.Columns.Contains(primaryKeyColumn))
                        {
                            throw new ArgumentException("All tables must have the specified primary-key column " + primaryKeyColumn, "primaryKeyColumn");
                        }
                    }
                }

                table.BeginLoadData(); // Turns off notifications, index maintenance, and constraints while loading data
                foreach (DataTable t in tables)
                {
                    table.Merge(t); // same as table.Merge(t, false, MissingSchemaAction.Add);
                }

                table.EndLoadData();

                if (primaryKeyColumn != null)
                {
                    // since we might have no real primary keys defined, the rows now might have repeating fields
                    // so now we're going to "join" these rows ...
                    var pkGroups = table.AsEnumerable()
                        .GroupBy(r => r[primaryKeyColumn]);
                    var dupGroups = pkGroups.Where(g => g.Count() > 1);
                    foreach (var grpDup in dupGroups)
                    {
                        // use first row and modify it
                        DataRow firstRow = grpDup.First();
                        foreach (DataColumn c in table.Columns)
                        {
                            if (firstRow.IsNull(c))
                            {
                                DataRow firstNotNullRow = grpDup.Skip(1).FirstOrDefault(r => !r.IsNull(c));
                                if (firstNotNullRow != null)
                                {
                                    firstRow[c] = firstNotNullRow[c];
                                }
                            }
                        }

                        // remove all but first row
                        var rowsToRemove = grpDup.Skip(1);
                        foreach (DataRow rowToRemove in rowsToRemove)
                        {
                            table.Rows.Remove(rowToRemove);
                        }
                    }

                    // remove primaryKeyColumn
                    table.Columns.Remove(primaryKeyColumn);
                }

                return table;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        public DataSet GetExcelWorkSheet(string filePath, string fileName, int workSheetNumber = 0)
        {
            try
            {
                OleDbConnection excelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + @"\" + fileName + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"");
                OleDbCommand excelCommand = new OleDbCommand();
                excelCommand.Connection = excelConnection;
                OleDbDataAdapter excelAdapter = new OleDbDataAdapter(excelCommand);

                excelConnection.Open();
                DataTable excelSheet = excelConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                int i = 0;
                string rangeColumn = CalacRangeColumns(i);
                string spreadSheetName = "[" + excelSheet.Rows[workSheetNumber]["TABLE_NAME"].ToString() + rangeColumn + "]";
                DataSet ds = new DataSet();
                bool keepRead = true;

                while (keepRead)
                {
                    excelCommand.CommandText = @"SELECT * FROM " + spreadSheetName;
                    DataTable dt = new DataTable();
                    dt.TableName = i.ToString();
                    try
                    {
                        excelAdapter.Fill(dt);
                        dt.Columns.Add("primeryKey", typeof(string));
                        for (int rowIndex = 1; rowIndex < dt.Rows.Count; rowIndex++)
                        {
                            dt.Rows[rowIndex]["primeryKey"] = rowIndex.ToString();
                        }

                        if (!ds.Tables.Contains(dt.TableName))
                        {
                            ds.Tables.Add(dt);
                        }

                        if (dt == null || dt.Columns == null || dt.Columns.Count == 0 || dt.Columns.Count < MAX_COLUMNS)
                        {
                            keepRead = false;
                        }
                        else
                        {
                            // get ranges 
                            i++;
                            rangeColumn = CalacRangeColumns(i);
                            spreadSheetName = "[" + excelSheet.Rows[workSheetNumber]["TABLE_NAME"].ToString() + rangeColumn + "]";
                        }
                    }
                    catch (OleDbException oleException)
                    {
                        keepRead = false;
                        log.Error("Excel Feeder Error - stop reading excel file - no more columns  " + oleException.Message + " ExcelReader");
                    }
                }

                excelConnection.Close();

                // merge all dt to one table in dataset
                DataTable mergedDt = new DataTable();
                List<DataTable> tables = new List<DataTable>();
                foreach (DataTable table in ds.Tables)
                {
                    tables.Add(table);
                }

                mergedDt = MergeAll(tables, "primeryKey");

                ds = new DataSet();
                ds.Tables.Add(mergedDt);

                return ds;
            }
            catch (Exception ex)
            {
                log.Error("Excel Feeder Error - Error opening Excel file " + ex.Message, ex);
                return null;
            }
        }

        #endregion

    }
}
