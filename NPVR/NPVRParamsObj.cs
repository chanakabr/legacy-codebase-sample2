using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRParamsObj
    {
        protected string assetID;
        protected string entityID;
        protected string accountID;
        protected long quota;
        protected DateTime startDate;
        protected string epgChannelID;
        protected bool isProtect; // true for protection, false for un-protection
        protected string streamType;
        protected string hasFormat;

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

        public virtual string AccountID
        {
            get
            {
                return accountID;
            }
            set
            {
                this.accountID = value;
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

        public virtual string StreamType
        {
            get
            {
                return streamType;
            }
            set
            {
                streamType = value;
            }
        }

        public virtual string HASFormat
        {
            get
            {
                return hasFormat;
            }
            set
            {
                hasFormat = value;
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
            sb.Append(String.Concat(" Stream Type: ", streamType));
            sb.Append(String.Concat(" HAS Format: ", hasFormat));
            return sb.ToString();
        }

        public string XkData { get; set; }
    }
}
