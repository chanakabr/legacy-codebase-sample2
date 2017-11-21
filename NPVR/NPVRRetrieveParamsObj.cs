using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR
{
    public class NPVRRetrieveParamsObj : NPVRParamsObj
    {
        protected List<SearchByField> searchBy;
        protected int pageIndex;
        protected int pageSize;
        protected NPVROrderDir direction;
        protected NPVROrderBy orderBy;
        protected List<NPVRRecordingStatus> recordingStatus;
        protected List<string> epgProgramIDs;
        protected List<string> assetIDs;
        protected List<string> seriesIDs;

        protected int seasonId;
        protected int seasonNumber;
        protected string type;

        public virtual List<SearchByField> SearchBy
        {
            get
            {
                if (searchBy == null)
                    searchBy = new List<SearchByField>();
                return searchBy;
            }
            set
            {
                searchBy = value;
            }
        }

        public virtual int PageIndex
        {
            get
            {
                return pageIndex;
            }
            set
            {
                if (value > -1)
                {
                    pageIndex = value;
                }
                else
                {
                    pageIndex = 0;
                }
            }
        }

        public virtual int PageSize
        {
            get
            {
                return pageSize;
            }
            set
            {
                if (value > 0)
                {
                    pageSize = value;
                }
                else
                {
                    pageSize = 0;
                }
            }
        }

        public virtual NPVROrderDir Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public virtual NPVROrderBy OrderBy
        {
            get
            {
                return orderBy;
            }
            set
            {
                orderBy = value;
            }
        }

        public virtual List<NPVRRecordingStatus> RecordingStatus
        {
            get
            {
                if (recordingStatus == null)
                    recordingStatus = new List<NPVRRecordingStatus>();
                return recordingStatus;
            }
            set
            {
                recordingStatus = value;
            }
        }

        public virtual List<string> EpgProgramIDs
        {
            get
            {
                if (epgProgramIDs == null)
                    epgProgramIDs = new List<string>();
                return epgProgramIDs;
            }
            set
            {
                epgProgramIDs = value;
            }
        }

        public virtual List<string> AssetIDs
        {
            get
            {
                if (assetIDs == null)
                    assetIDs = new List<string>();
                return assetIDs;
            }
            set
            {
                assetIDs = value;
            }
        }

        public virtual List<string> SeriesIDs
        {
            get
            {
                if (seriesIDs == null)
                    seriesIDs = new List<string>();
                return seriesIDs;
            }
            set
            {
                seriesIDs = value;
            }
        }

        public virtual int SeasonNumber
        {
            get
            {
                return seasonNumber;
            }
            set
            {
                seasonNumber = value;
            }
        }

        public virtual int SeasonId
        {
            get
            {
                return seasonId;
            }
            set
            {
                seasonId = value;
            }
        }
        public virtual string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }

        }
              
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("NPVRRetrieveParamsObj. Base Obj:", base.ToString()));
            sb.Append(String.Concat(" SearchBy Count: ", SearchBy.Count, " "));
            if (SearchBy.Count > 0)
            {
                for (int i = 0; i < SearchBy.Count; i++)
                {
                    sb.Append(String.Concat(SearchBy[i].ToString(), ";"));
                }
            }
            sb.Append(String.Concat(" Page Index: ", PageIndex));
            sb.Append(String.Concat(" Page Size: ", PageSize));
            sb.Append(String.Concat(" Order By: ", OrderBy.ToString()));
            sb.Append(String.Concat(" Order Dir: ", Direction.ToString()));
            sb.Append(String.Concat(" Recording Status Count: ", RecordingStatus.Count));
            if (RecordingStatus.Count > 0)
            {
                for (int i = 0; i < RecordingStatus.Count; i++)
                {
                    sb.Append(String.Concat(RecordingStatus[0], ";"));
                }
            }
            sb.Append(String.Concat(" seriesIDs: ", SeriesIDs != null && SeriesIDs.Count > 0 ? string.Join(",",SeriesIDs) : string.Empty));

            sb.Append(String.Concat(" SeasonNumber : ", SeasonNumber));
            sb.Append(String.Concat(" SeasonId : ", SeasonId));
            sb.Append(String.Concat(" Type : ", Type));

            return sb.ToString();
        }

        public List<SearchByField> GetUniqueSearchBy()
        {
            return SearchBy.Distinct().ToList();
        }

        public List<NPVRRecordingStatus> GetUniqueRecordingStatuses()
        {
            return RecordingStatus.Distinct().ToList();
        }
    }
}
