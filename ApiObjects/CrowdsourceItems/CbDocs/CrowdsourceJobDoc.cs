using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.CouchbaseWrapperObjects;
using Newtonsoft.Json;

namespace ApiObjects.CrowdsourceItems.CbDocs
{
    public class CrowdsourceJobDoc : CbDocumentBase
    {
        private int _groupId;
        private eCrowdsourceType _type;
        private int _assetId;

        public CrowdsourceJobDoc(int groupId, eCrowdsourceType type, int assetId)
        {
            this._groupId = groupId;
            this._type = type;
            this._assetId = assetId;
        }

        public override string Id
        {
            get { return string.Format("job::{0}:{1}:{2}", _groupId, _type, _assetId); }
        }

        [JsonProperty("lastItemId")]
        public int LastItemId { get; set; }
    }
}
