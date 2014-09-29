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

        public List<string> m_lTagsName;
        public List<string> m_lMetasName;

        #endregion

        public EpgGroupSettings()
        {
            m_lTagsName = new List<string>();
            m_lMetasName = new List<string>();
        }
    }
}
