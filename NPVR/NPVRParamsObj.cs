using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRParamsObj
    {
        private string assetID;
        private string entityID;
        private long quota;

        public virtual string AssetID
        {
            get
            {
                return assetID;
            }
            set
            {
                this.assetID = value;
            }
        }

        public virtual string EntityID
        {
            get
            {
                return entityID;
            }
            set
            {
                this.entityID = value;
            }
        }

        public virtual long Quota // quota is in minutes.
        {
            get
            {
                return quota;
            }
            set
            {
                this.quota = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("NPVRParamsObj. ");
            sb.Append(String.Concat("Asset ID: ", AssetID));
            sb.Append(String.Concat(" Entity ID: ", EntityID));
            sb.Append(String.Concat(" Quota: ", Quota));
            return sb.ToString();
        }
    }
}
