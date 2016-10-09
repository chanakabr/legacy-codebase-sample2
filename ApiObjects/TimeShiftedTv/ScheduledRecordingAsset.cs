using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class ScheduledRecordingAsset
    {

        public long AssetId { get; set; }

        public RecordingType Type { get; set; }

        public string Crid { get; set; }

        public ScheduledRecordingAsset() { }

        public ScheduledRecordingAsset(long assetId, RecordingType type, string crid)
        {
            this.AssetId = assetId;
            this.Type = type;
            this.Crid = crid;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("AssetId: {0}", AssetId));
            sb.Append(string.Format("Type: {0}", Type));
            sb.Append(string.Format("Crid: {0}", string.IsNullOrEmpty(Crid) ? "" : Crid));

            return sb.ToString();
        }
    }
}
