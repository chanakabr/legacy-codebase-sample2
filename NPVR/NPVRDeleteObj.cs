using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRDeleteObj : NPVRParamsObj
    {
        public List<NPVRRecordingStatus> Status { get; set; }

        public string SeriesID { get; set; }

        public int SeasonNumber { get; set; }

        public string ChannelId { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("NPVRDeleteObj. Base Obj:", base.ToString()));
            sb.Append(String.Concat("Status: ", string.Join(",", Status.Select(x=>x.ToString().ToLower())))); 
            sb.Append(String.Concat(" SeriesID: ", SeriesID));
            sb.Append(String.Concat(" SeasonNumber: ", SeasonNumber));            
            sb.Append(String.Concat(" ChannelId: ", ChannelId));
            
            return sb.ToString();
        }


    }
}
