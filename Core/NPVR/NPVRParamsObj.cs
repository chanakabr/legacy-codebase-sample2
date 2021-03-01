using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRParamsObj
    {
        protected List<string> _statuses = new List<string> { "scheduled", "ongoing", "completed", "cancelled" };
        protected string assetID; //allows to specify the list of identifiers of the bookings involved in this operation
        protected string entityID;
        protected string accountID;
        protected long quota;
        protected DateTime startDate;
        protected string epgChannelID; //allows filtering only the bookings associated to a given channelId(s).
        protected bool isProtect; // true for protection, false for un-protection
        protected string streamType;
        protected string hasFormat;

        protected bool deleteProtected; //Indicates if protected recordings should be removed or not
        protected bool deleteBookings; //Indicates if incomplete recordings (scheduled or ongoing) should be removed or not

        protected string byAlreadyWatched; //allows to specify the list of values in the alreadyWatched involved in this operation.
        protected string byProgramId; //allows to specify the list of program identifiers of the bookings involved in this operation.
        protected bool deleteOngoingRecordings; //Flag to indicate if ongoing recording(s) to be removed or not.
        protected string byStatus; //To indicate the status of the recordings to be deleted.

        protected int value;

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

        public virtual int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public virtual bool DeleteProtected
        {
            get
            {
                return deleteProtected;
            }
            set
            {
                deleteProtected = value;
            }
        }

        public virtual bool DeleteBookings
        {
            get
            {
                return deleteBookings;
            }
            set
            {
                deleteBookings = value;
            }
        }

        public virtual string ByAlreadyWatched
        {
            get
            {
                return byAlreadyWatched;
            }
            set
            {
                byAlreadyWatched = value;
            }
        }

        public virtual string ByProgramId
        {
            get
            {
                return byProgramId;
            }
            set
            {
                byProgramId = value;
            }
        }

        public virtual bool DeleteOngoingRecordings
        {
            get
            {
                return deleteOngoingRecordings;
            }
            set
            {
                deleteOngoingRecordings = value;
            }
        }

        public virtual string ByStatus
        {
            get
            {
                return byStatus;
            }
            set
            {
                byStatus = value;
            }
        }

        public string GetValidStatuses()
        {
            if (string.IsNullOrEmpty(ByStatus))
                return string.Empty;

            var _list = ByStatus.Split(',').Select(x => x.Trim().ToLower()).ToList();
            var _byStatus = _list.Where(s => _statuses.Contains(s))
                                 .Select(s => s)
                                 .Distinct().ToList();
            return string.Join(",", _byStatus);
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
            sb.Append(String.Concat(" Value: ", value));
            sb.Append(String.Concat(" DeleteProtected: ", deleteProtected.ToString().ToLower()));
            sb.Append(String.Concat(" DeleteBookings: ", deleteBookings.ToString().ToLower()));
            sb.Append(String.Concat(" ByAlreadyWatched: ", byAlreadyWatched));
            sb.Append(String.Concat(" ByProgramId: ", byProgramId));
            sb.Append(String.Concat(" ByStatus: ", GetValidStatuses()));
            sb.Append(String.Concat(" DeleteOngoingRecordings: ", deleteOngoingRecordings.ToString().ToLower()));
            return sb.ToString();
        }

        public string XkData { get; set; }
    }
}
