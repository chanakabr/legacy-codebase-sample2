using System.Collections.Generic;

namespace WebAPI.Managers.Models
{
    public class KSUtils
    {
        public const string PAYLOAD_UDID = "UDID";
        public const string PAYLOAD_CREATE_DATE = "CreateDate";
        public const string PAYLOAD_REGION = "Region";

        public static string PrepareKSPayload(KS.KSData pl)
        {
            var ksDataList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(PAYLOAD_UDID, pl.UDID),
                new KeyValuePair<string, string>(PAYLOAD_CREATE_DATE, pl.CreateDate.ToString()),
                new KeyValuePair<string, string>(PAYLOAD_REGION, pl.RegionId.ToString())
            };
            var payload = KS.preparePayloadData(ksDataList);
            return payload;
        }

        public static KS.KSData ExtractKSPayload(KS ks)
        {
            if (ks == null)
            {
                return new KS.KSData();
            }

            var pl = KS.ExtractPayloadData(ks.Data);

            string udid = "";
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

            return new KS.KSData(udid, createDate, regionId);
        }

        internal static KS.KSData ExtractKSPayload()
        {
            return ExtractKSPayload(KS.GetFromRequest());
        }
    }
}