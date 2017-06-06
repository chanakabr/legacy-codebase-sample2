using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

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

        public Dictionary<long, string> tags;
        public Dictionary<long, string> metas;

        #endregion

        public EpgGroupSettings()
        {
            m_lTagsName = new List<string>();
            m_lMetasName = new List<string>();

            tags = new Dictionary<long, string>();
            metas = new Dictionary<long, string>();

            GroupId = 0;
        }
    }
}
