using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class UserSocialActionRequest
    {
        public eAssetType AssetType { get; set; }

        public string DeviceUDID { get; set; }

        public string SiteGuid { get; set; }

        public int AssetID { get; set; }

        public eUserAction Action { get; set; }

        //public SocialPlatform network { get; set; }

        public UserSocialActionRequest()
        {
            SiteGuid = string.Empty;
            AssetID = 0;            
            Action = eUserAction.UNKNOWN;
            AssetType = eAssetType.UNKNOWN;
        }

        public override string ToString()
        {
            string res = string.Format("AssetType : {0}, DeviceUDID :{1}, SiteGuid : {2}, AssetID :{3}, Action : {4}", AssetType.ToString(), DeviceUDID, SiteGuid, AssetID, Action.ToString());
            return res;
        }

    }
}
