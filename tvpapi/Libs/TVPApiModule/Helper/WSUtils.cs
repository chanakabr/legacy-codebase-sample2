using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi.ODBCWrapper;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects;

namespace TVPApi
{
    public class WSUtils
    {

        public static int GetGroupIDByMediaType(int mediaType)
        {
            int retVal = 0;
            ConnectionManager connManager = new ConnectionManager(0, PlatformType.Unknown, false);
            DataSetSelectQuery selectQuery = new DataSetSelectQuery();
            selectQuery.SetConnectionString(connManager.GetTvinciConnectionString());
            selectQuery += "select group_id from media_types ";
            selectQuery += " where ";
            selectQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("id", "=", mediaType);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                }
            }
            return retVal;
        }

        public static IImplementation GetImplementation(int nGroupID, InitializationObject initObj)
        {
            switch (nGroupID)
            {
                case 153:
                    return new ImplementationYes(nGroupID, initObj);
                
                default:
                    return new ImplementationBase(nGroupID, initObj);
            }
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
