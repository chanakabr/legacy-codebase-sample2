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

        public eAssetTypes AssetType { get; set; }

        public DateTime UpdatedDate { get; set; }

        public ScheduledRecordingAsset() { }

        public ScheduledRecordingAsset(long assetId, eAssetTypes AssetType, DateTime UpdatedDate)
        {
            this.AssetId = assetId;
            this.AssetType = AssetType;
            this.UpdatedDate = UpdatedDate;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("AssetId: {0}", AssetId));
            sb.Append(string.Format("AssetType: {0}", AssetType));
            sb.Append(string.Format("UpdatedDate: {0}", UpdatedDate));

            return sb.ToString();
        }
    }
}
