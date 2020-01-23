using KSWrapper;
using System;
using System.Text;
using TVPApiModule.Helper;
using TVPApiModule.Objects.Authorization;

namespace TVPApiModule.Manager
{
    public class KSManager
    {
        public static KS ParseKS(string ks)
        {
            StringBuilder sb = new StringBuilder(ks);
            sb = sb.Replace("-", "+");
            sb = sb.Replace("_", "/");

            int groupId = 0;
            byte[] encryptedData = null;
            string encryptedDataStr = null;
            string[] ksParts = null;

            try
            {
                encryptedData = Convert.FromBase64String(sb.ToString());
                encryptedDataStr = Encoding.ASCII.GetString(encryptedData);
                ksParts = encryptedDataStr.Split('|');
            }
            catch (Exception)
            {
                throw new Exception(AuthorizationManager.INVALID_KS_FORMAT_ERROR);
            }

            if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                throw new Exception(AuthorizationManager.INVALID_KS_FORMAT_ERROR);
            }

            Group group = GroupsManager.GetGroup(groupId);
            string adminSecret = group.UserSecret;

            // build KS
            string fallbackSecret = group.UserSecretFallbackExpiryEpoch > TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow) ? group.UserSecretFallback : null;

            try
            {
                return new KS(ks, groupId, encryptedData, adminSecret, fallbackSecret);
            }
            catch (FormatException)
            {
                throw new Exception(AuthorizationManager.INVALID_KS_FORMAT_ERROR);
            }
        }
    }

    public enum KalturaSessionType
    {
        USER = 0, ADMIN = 2
    }
}