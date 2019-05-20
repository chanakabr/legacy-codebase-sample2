using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GroupsCacheManager
{
    [Serializable]
    [JsonObject(Id = "epggroupsettings")]
    public class EpgGroupSettings
    {
        #region members

        public int GroupId;

        public List<string> m_lTagsName;
        public List<string> m_lMetasName;

        public List<string> MetasDisplayName;
        public List<string> TagsDisplayName;

        public Dictionary<long, string> tags;
        public Dictionary<long, string> metas;

        #endregion

        public EpgGroupSettings()
        {
            m_lTagsName = new List<string>();
            m_lMetasName = new List<string>();
            MetasDisplayName = new List<string>();
            TagsDisplayName = new List<string>();

            tags = new Dictionary<long, string>();
            metas = new Dictionary<long, string>();

            GroupId = 0;
        }
    }
}
