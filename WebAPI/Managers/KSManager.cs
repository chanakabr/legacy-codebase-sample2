using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using KLogMonitor;
using TVinciShared;
using ApiObjects.Base;
using WebAPI.Utils;
using System.Text.RegularExpressions;
using WebAPI.ClientManagers;
using ConfigurationManager;
using Newtonsoft.Json;
using KSWrapper;
using WebAPI.Managers.Models;

namespace WebAPI.Managers
{
    // TODO SHIR - REMOVE AFTER UPDATE NUGET
    public static class ksex
    {
        
        public static KSData ExtractKSData(this KS ks)
        {
            return null;
        }
    }

    public class KSManager
    {
        // TODO SHIR - CHECK IF REALY NEED THIS OR UST CONTEXTDATA
        internal static KS GetKSFromRequest()
        {
            return (KS)HttpContext.Current.Items[RequestContextUtils.REQUEST_KS];
        }

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
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            var group = GroupsManager.GetGroup(groupId);
            string adminSecret = group.UserSecret;

            // build KS
            string fallbackSecret = group.UserSecretFallbackExpiryEpoch > DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) ? group.UserSecretFallback : null;
            // TODO SHIR
            //return new KS(ks, groupId, encryptedData, adminSecret, fallbackSecret);
            return null;
        }

        internal static void SaveOnRequest(KS ks, bool saveConstantsGroupId)
        {
            if (HttpContext.Current.Items.ContainsKey(RequestContextUtils.REQUEST_KS))
                HttpContext.Current.Items[RequestContextUtils.REQUEST_KS] = ks;
            else
                HttpContext.Current.Items.Add(RequestContextUtils.REQUEST_KS, ks);

            if (HttpContext.Current.Items.ContainsKey(RequestContextUtils.REQUEST_GROUP_ID))
                HttpContext.Current.Items[RequestContextUtils.REQUEST_GROUP_ID] = ks.GroupId;
            else
                HttpContext.Current.Items.Add(RequestContextUtils.REQUEST_GROUP_ID, ks.GroupId);

            if (saveConstantsGroupId)
            {
                if (HttpContext.Current.Items.ContainsKey(Constants.GROUP_ID))
                    HttpContext.Current.Items[Constants.GROUP_ID] = ks.GroupId;
                else
                    HttpContext.Current.Items.Add(Constants.GROUP_ID, ks.GroupId);
            }
            else
            {
                // TODO SHIR
                //if (!string.IsNullOrEmpty(ks.OriginalUserId) && ks.OriginalUserId != ks.UserId && long.TryParse(ks.OriginalUserId, out long originalUserId))
                //{
                //    if (HttpContext.Current.Items.ContainsKey(RequestContextUtils.REQUEST_KS_ORIGINAL_USER_ID))
                //        HttpContext.Current.Items[RequestContextUtils.REQUEST_KS_ORIGINAL_USER_ID] = originalUserId;
                //    else
                //        HttpContext.Current.Items.Add(RequestContextUtils.REQUEST_KS_ORIGINAL_USER_ID, originalUserId);
                //}
            }
        }

        internal static void ClearOnRequest()
        {
            HttpContext.Current.Items.Remove(RequestContextUtils.REQUEST_GROUP_ID);
            HttpContext.Current.Items.Remove(RequestContextUtils.REQUEST_KS);
        }

        public static ContextData GetContextData()
        {
            var ks = GetKSFromRequest();
            long? domainId = null, userId = null;

            try
            {
                domainId = HouseholdUtils.GetHouseholdIDByKS();
            }
            catch (Exception) { }

            try
            {
                userId = Utils.Utils.GetUserIdFromKs(ks);
            }
            catch (Exception) { }

            var contextData = new ContextData(ks.GroupId)
            {
                DomainId = domainId,
                UserId = userId
            };

            return contextData;
        }
    }
}