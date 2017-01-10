using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Managers.Models;

namespace WebAPI.Utils
{
    public static class DrmUtils
    {
        public static string BuildCencCustomDataString(int fileId)
        {
            string response = null;

            KS ks = KS.GetFromRequest();
            Group group = GroupsManager.GetGroup(ks.GroupId);

            CencCustomData customData = new CencCustomData()
            {
                AccountId = ks.GroupId,
                CaSystem = "OTT",
                Files = string.Empty,
                UserToken = ks.ToString(),
                ContentId = fileId,
            };

            response = JsonConvert.SerializeObject(customData);

            return Convert.ToBase64String(Encoding.ASCII.GetBytes(response));

            return response;
        }

        public static string BuildCencSignatureString(string customDataString)
        {
            string response = null;

            KS ks = KS.GetFromRequest();
            Group group = GroupsManager.GetGroup(ks.GroupId);

            response = string.Concat(group.AccountPrivateKey, customDataString);

            return HttpUtility.UrlDecode(Convert.ToBase64String(EncryptionUtils.HashSHA1(Encoding.ASCII.GetBytes(response))));
        }
    }
}