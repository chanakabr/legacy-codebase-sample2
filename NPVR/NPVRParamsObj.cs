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
        private DateTime startDate;
        private string epgChannelID;
        private bool isProtect; // true for protection, false for un-protection

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

        public virtual long Quota // quota is in seconds.
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

        public virtual DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                this.startDate = value;
            }
        }

        public virtual string EpgChannelID
        {
            get
            {
                return epgChannelID;
            }
            set
            {
                this.epgChannelID = value;
            }
        }

        public virtual bool IsProtect
        {
            get
            {
                return isProtect;
            }
            set
            {
                this.isProtect = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("NPVRParamsObj. ");
            sb.Append(String.Concat("Asset ID: ", AssetID));
            sb.Append(String.Concat(" Entity ID: ", EntityID));
            sb.Append(String.Concat(" Quota: ", Quota));
            sb.Append(String.Concat(" Start Date: ", startDate.ToString("yyyyMMddHHmmss")));
            sb.Append(String.Concat(" Epg Channel ID: ", epgChannelID));
            sb.Append(String.Concat(" Is Protect: ", IsProtect.ToString().ToLower()));
            return sb.ToString();
        }
    }
}
