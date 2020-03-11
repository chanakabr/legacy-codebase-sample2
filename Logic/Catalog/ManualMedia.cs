using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Core.Catalog
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "manualmedia")]
    public class ManualMedia
    {
        #region Members
        [DataMember]
        public string m_sMediaId { get; set; }
        [DataMember]        
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
