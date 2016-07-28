using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODBCWrapper;

namespace ps_notifiers
{
    public class Utils
    {
        public static Dictionary<string, string> GetMediaCoGuidEntryId(int mediaID)
        {
            Dictionary<string, string> mediaCoGuidEntryId = new Dictionary<string, string>();

            DataSetSelectQuery dataSetSelectQuery = new DataSetSelectQuery();
            dataSetSelectQuery.SetCachedSec(0);
            dataSetSelectQuery += "select media.co_guid, media.entry_id from media WITH (nolock) ";
            dataSetSelectQuery += "where ";
            dataSetSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("media.id", "=", mediaID);
            if (dataSetSelectQuery.Execute("query", true) != null)
            {
                int count = dataSetSelectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    string mediaCoGuid = dataSetSelectQuery.Table("query").DefaultView[0].Row["co_guid"].ToString();
                    string mediaEntryId = dataSetSelectQuery.Table("query").DefaultView[0].Row["entry_id"].ToString();

                    mediaCoGuidEntryId.Add(mediaCoGuid, mediaEntryId);
                }
            }
            dataSetSelectQuery.Finish();
            dataSetSelectQuery = null;

            return mediaCoGuidEntryId;
        }
    }
}
