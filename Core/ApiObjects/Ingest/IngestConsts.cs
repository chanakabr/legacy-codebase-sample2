using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Ingest
{
    public static class IngestConsts
    {
        public static string GetEpgV2SearchIndex(int groupId, long bulkId)
        {
            return $"{groupId}_epg_v2_*_{bulkId}";
        }
        public static readonly string LAST_BACKUP_ALIAS = "LAST_BACKUP";
        public static readonly string PURGE_INDEX_ALIAS = "PURGE";
    }
    
}
