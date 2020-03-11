using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Odbc;
using TVPApi.ODBCWrapper;
using System.Data;
using System.Reflection;
using Tvinci.Performance;
using KLogMonitor;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public class DatabaseProvider : LoaderProvider<IDatabaseAdapter>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        Connection m_conn = new Connection();
        public override object GetDataFromSource(IDatabaseAdapter adapter)
        {
            TVPApi.ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery();

            adapter.ExecuteInitializeQuery(query);
            Guid requestGuid = Guid.NewGuid();
            //if (logger.IsDebugEnabled)
#if DEBUG
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
#endif
                     
            query.SetCachedSec(0);    
        
            DataTable result = null;
            try
            {
                using (KMonitor mon = new KMonitor(Events.eEvent.EVENT_DATABASE) { Table = "table" })
                {
                    result = query.Execute("table", true);              
                }      
            }
            catch (Exception)
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
