using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Catalog
{
    [Serializable]
    [JsonObject(Id = "manualmedia")]
    public class ManualMedia
    {
        #region Members

        public string m_sMediaId { get; set; }
        public int m_nOrderNum { get; set; }

        #endregion

        #region CTOR

        public ManualMedia(string sMediaId, int nOrderNum)
        {
            this.m_sMediaId = sMediaId;
            this.m_nOrderNum = nOrderNum;
        }

        #endregion
    }
}
