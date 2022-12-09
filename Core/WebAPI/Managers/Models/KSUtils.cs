using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.Utils;

namespace WebAPI.Managers.Models
{
    public class KSUtils
    {
        public const string PAYLOAD_UDID = "UDID";
        public const string PAYLOAD_CREATE_DATE = "CreateDate";
        public const string PAYLOAD_REGION = "r";
        public const string PAYLOAD_USER_SEGMENTS = "us";
        public const string PAYLOAD_USER_ROLES = "ur";
        public const string PAYLOAD_SESSION_CHARACTERISTIC_KEY = "sck";
        public const string PAYLOAD_SIGNATURE = "sig";
        public const string PAYLOAD_DOMAINID = "hh";
        public const string PAYLOAD_IS_BYPASS_CACHE_ELIGIBLE = "bce";

        private const string ONE = "1";

        public static string PrepareKSPayload(KS.KSData pl)
        {
            var ksDataList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(PAYLOAD_UDID, pl.UDID),
                new KeyValuePair<string, string>(PAYLOAD_CREATE_DATE, pl.CreateDate.ToString()),
                new KeyValuePair<string, string>(PAYLOAD_REGION, pl.RegionId.ToString())
            };

            if (pl.UserSegments != null)
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_USER_SEGMENTS, string.Join(",", pl.UserSegments.OrderBy(x => x))));
            }

            if (pl.UserRoles != null)
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_USER_ROLES, string.Join(",", pl.UserRoles.OrderBy(x => x))));
            }
            
            if (!pl.SessionCharacteristicKey.IsNullOrEmpty())
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_SESSION_CHARACTERISTIC_KEY, pl.SessionCharacteristicKey));
            }

            if (pl.DomainId > 0)
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_DOMAINID, pl.DomainId.ToString()));
            }
            
            if (!string.IsNullOrEmpty(pl.Signature))
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_SIGNATURE, pl.Signature));
            }

            if (pl.IsBypassCacheEligible)
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_IS_BYPASS_CACHE_ELIGIBLE, ONE));
            }

            return KS.preparePayloadData(ksDataList);
        }

        public static KS.KSData ExtractKSPayload(KS ks)
        {
            if (ks == null)
            {
                return KS.KSData.Empty;
            }

            var pl = KS.ExtractPayloadData(ks.Data);

            string udid = string.Empty;
            if (pl.ContainsKey(PAYLOAD_UDID))
            {
                udid = pl[PAYLOAD_UDID];
            }

            int createDate = 0;
            if (pl.ContainsKey(PAYLOAD_CREATE_DATE))
            {
                int.TryParse(pl[PAYLOAD_CREATE_DATE], out createDate);
            }

            int regionId = 0;
            if (pl.ContainsKey(PAYLOAD_REGION))
            {
                int.TryParse(pl[PAYLOAD_REGION], out regionId);
            }

            List<long> userSegments = new List<long>();
            if (pl.ContainsKey(PAYLOAD_USER_SEGMENTS))
            {
                userSegments.AddRange(pl[PAYLOAD_USER_SEGMENTS].GetItemsIn<long>(out _));
            }

            List<long> userRoles = new List<long>();
            if (pl.ContainsKey(PAYLOAD_USER_ROLES))
            {
                userRoles.AddRange(pl[PAYLOAD_USER_ROLES].GetItemsIn<long>(out _));
            } 

            pl.TryGetValue(PAYLOAD_SESSION_CHARACTERISTIC_KEY, out var sessionCharacteristicKey);

            int domainId = 0;
            if (pl.ContainsKey(PAYLOAD_DOMAINID))
            {
                int.TryParse(pl[PAYLOAD_DOMAINID], out domainId);
            }

            var signature = string.Empty;
            if (pl.ContainsKey(PAYLOAD_SIGNATURE))
            {
                signature = pl[PAYLOAD_SIGNATURE];
            }

            var isBypassCacheEligible = pl.ContainsKey(PAYLOAD_IS_BYPASS_CACHE_ELIGIBLE);

            return new KS.KSData(
                udid,
                createDate,
                regionId,
                userSegments,
                userRoles,
                sessionCharacteristicKey,
                domainId,
                isBypassCacheEligible,
                signature);
        }

        internal static KS.KSData ExtractKSPayload()
        {
            return ExtractKSPayload(KS.GetFromRequest());
        }
    }
}