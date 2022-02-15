using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using Phx.Lib.Log;
using System.Reflection;

namespace XMLAdapter
{
    public sealed class ADI3Adapter : BaseXMLAdapter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // handle genre and languages excel files
        private DataSet m_ExcelGenreDS;
        private DataSet m_ExcelLanguagesDS;

        private int m_offerNumber = 0;

        // TODO: later on, read these configuration information from outside
        string EXCEL_GENRE_FILE_PATH = ConfigurationManager.AppSettings["XSL_PATH"].ToString() + "\\";
        string EXCEL_LANGUAGES_FILE_PATH = ConfigurationManager.AppSettings["XSL_PATH"].ToString() + "\\";
        const string EXCEL_LANGUAGES_FILE_NAME = "Languages_table.xls";
        const string EXCEL_GENRE_FILE_NAME = "Genre.xls";

        // callback function for handling genre information
        public override string HandleGenre(string filter, string language)
        {
            string[] sSplit = null;
            try
            {
                sSplit = filter.Split(new char[] { ':' }, 2);
                if (sSplit[0] == "dvb" && sSplit[1] != "0/0")
                {
                    return HandleGenreReq(filter, false, language);
                }
                else
                {
                    return string.Empty;
                }

            }
            catch
            {
                return string.Empty;
            }
        }

        public override string GetOfferNumber()
        {
            return m_offerNumber.ToString();
        }

        public void SetOfferNumber(int offerNumber)
        {
            m_offerNumber = offerNumber;
        }

        // callback function for handling subgenre information
        public override string HandleSubGenre(string filter, string language)
        {
            string[] sSplit = null;
            try
            {
                sSplit = filter.Split(new char[] { ':' }, 2);
                if (sSplit[0] == "dvb" && sSplit[1] != "0/0")
                {
                    return HandleGenreReq(filter, true, language);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public override string ParseDateValue(string date)
        {
            string sDate = string.Empty;

            if (!string.IsNullOrEmpty(date))
            {
                string format = "yyyy-MM-ddTHH:mm:ssZ";
                DateTime dDate = DateTime.ParseExact(date, format, null);
                sDate = dDate.ToString("dd/MM/yyyy HH:mm:ss");
            }

            return sDate;
        }

        // callback function for handling languages information
        public override string HandleLanguage(string lang)
        {
            string sRVal = lang;
            string sExpression = string.Empty;

            sExpression = "Languages = '" + lang + "'";
            DataTable dd = m_ExcelLanguagesDS.Tables[0].Copy();
            DataRow[] dr = dd.Select(sExpression);

            sRVal = dr[0]["Languages trnsformed"] as string;

            return sRVal;
        }

        // dictionary for translate media types
        Dictionary<string, string> m_mediaTypesTranslator = new Dictionary<string, string>()
        {
            {"S","Episode"},
            {"M","Movie"},
            {"C","Series"}
        };

        // parse rating
        public override string HandleRating(string code)
        {
            string[] sSplit = null;
            try
            {
                sSplit = code.Split(new char[] { ':' }, 2);
                if (sSplit[0] == "private")
                {
                    return sSplit[1];
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        // parse parental
        public override string HandleParental(string code)
        {
            string[] sSplit = null;
            try
            {
                sSplit = code.Split(new char[] { ':' }, 2);
                if (sSplit[0] == "dvb")
                {
                    return sSplit[1];
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        // divide price
        public override string HandleDiv(string number, string divider)
        {
            string ret = "";

            double priceD = double.Parse(number);
            priceD /= int.Parse(divider);

            ret = priceD.ToString();

            return ret;
        }

        // parse media type code and return the result
        public override string HandleMediaType(string code)
        {
            string sRVal = string.Empty;
            string[] sSplit = null;

            try
            {
                sSplit = code.Split(new char[] { ':' }, 2);
                if (sSplit[0] == "private")
                {
                    sRVal = m_mediaTypesTranslator[sSplit[1]];
                }
            }
            catch
            {
                return string.Empty;
            }

            return sRVal;
        }

        // search the genre information in the excel table
        private string HandleGenreReq(string filter, bool isSub, string language)
        {
            string sRVal = filter;
            string sExpression = string.Empty;

            string[] sSplit = filter.Split(new char[] { ':' }, 2);
            string[] sSplitGenre = sSplit[1].Split(new char[] { '/' }, 2);
            string sGenreID = sSplitGenre[0];
            string sSubGenreID = sSplitGenre[1];

            // select all the rows which have the genre id  and the subgenre filter arguments
            sExpression = "GENRE_ID = " + sGenreID + "and SUB_GENRE_ID = " + sSubGenreID;

            DataTable dd = m_ExcelGenreDS.Tables[0].Copy();
            DataRow[] dr = dd.Select(sExpression);

            if (isSub == true)
            {
                // get the subgenre
                if (language == "eng")
                {
                    sRVal = dr[0]["SUB_GENRE_DESCR_ENG"] as string;
                }
                else if (language == "rus")
                {
                    sRVal = dr[0]["SUB_GENRE_DESCR_RUS"] as string;
                }
                else if (language == "heb")
                {
                    sRVal = dr[0]["SUB_GENRE_DESCR_HEB"] as string;
                }
            }
            else
            {
                // get the genre
                if (language == "eng")
                {
                    sRVal = dr[0]["GENRE_DESCR_ENG"] as string;
                }
                else if (language == "rus")
                {
                    sRVal = dr[0]["GENRE_DESCR_RUS"] as string;
                }
                else if (language == "heb")
                {
                    sRVal = dr[0]["GENRE_DESCR_HEB"] as string;
                }
            }

            return sRVal;
        }

        // handle the translation excel files and init base
        public override void Init()
        {
            m_ExcelGenreDS = GetExcelWorkSheet(EXCEL_GENRE_FILE_PATH, EXCEL_GENRE_FILE_NAME, 0).Copy();
            m_ExcelLanguagesDS = GetExcelWorkSheet(EXCEL_LANGUAGES_FILE_PATH, EXCEL_LANGUAGES_FILE_NAME, 0).Copy();

            base.Init(); // Init base
        }

        private DataSet GetExcelWorkSheet(string pathName, string fileName, int workSheetNumber)
        {
            try
            {
                OleDbConnection ExcelConnection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + @"\" + fileName + ";Extended Properties=Excel 8.0;");
                OleDbCommand ExcelCommand = new OleDbCommand();
                ExcelCommand.Connection = ExcelConnection;
                OleDbDataAdapter ExcelAdapter = new OleDbDataAdapter(ExcelCommand);

                ExcelConnection.Open();
                DataTable ExcelSheets = ExcelConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string SpreadSheetName = "[" + ExcelSheets.Rows[workSheetNumber]["TABLE_NAME"].ToString() + "]";

                DataSet ExcelDataSet = new DataSet();
                ExcelCommand.CommandText = @"SELECT * FROM " + SpreadSheetName;
                ExcelAdapter.Fill(ExcelDataSet);

                ExcelConnection.Close();
                return ExcelDataSet;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return null;
            }
        }
    }
}
