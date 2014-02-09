using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Odbc;
using ODBCWrapper;
using System.Data;
using log4net;
using System.Reflection;
using Tvinci.Performance;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public class DatabaseProvider : LoaderProvider<IDatabaseAdapter>
    {
        public static ILog logger = log4net.LogManager.GetLogger(typeof(DatabaseProvider));

        Connection m_conn = new Connection();
        public override object GetDataFromSource(IDatabaseAdapter adapter)
        {
            ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery();

            adapter.ExecuteInitializeQuery(query);
            Guid requestGuid = Guid.NewGuid();
            if (logger.IsDebugEnabled)
            {
                FieldInfo statementInfo = query.GetType().BaseType.BaseType.GetField("m_sOraStr", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance);
                string statement = (string)statementInfo.GetValue(query);
                FieldInfo parametersInfo = query.GetType().BaseType.BaseType.GetField("m_hashTable", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance);
                object[] parameters = (object[])parametersInfo.GetValue(query);

                StringBuilder sb = new StringBuilder();


                int i = 0;
                object value = parameters[i];

                while (value != null)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(value.ToString());

                    i++;
                    value = parameters[i];
                }


                logger.DebugFormat("{0}SQL - {3}{0}{1}{0}{0}Parameters{0}{2}", "\r\n", statement, sb.ToString(), requestGuid.ToString());                
            }
                     
            query.SetCachedSec(0);    
        
            DataTable result = null;
            try
            {
                using (TvinciStopwatch timer = new TvinciStopwatch(ePerformanceSource.Site, string.Concat("Database Request - ", requestGuid.ToString())))
                {
                    result = query.Execute("table", true);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                query.Finish();
            }
            
            
            if (result == null)
            {
                // TODO - check with guy what should do if having problems
                return null;
            }
            else
            {
                return result;
            }
        }      
    }
}
