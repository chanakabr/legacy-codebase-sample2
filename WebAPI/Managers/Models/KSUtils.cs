using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Utils;

namespace WebAPI.Managers.Models
{
    public class KSUtils
    {
        public const string PAYLOAD_UDID = "UDID";
        public const string PAYLOAD_CREATE_DATE = "CreateDate";

        public static string PrepareKSPayload(WebAPI.Managers.Models.KS.KSData pl)
        {
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(PAYLOAD_UDID, pl.UDID));
            l.Add(new KeyValuePair<string, string>(PAYLOAD_CREATE_DATE, pl.CreateDate.ToString()));
            string payload = WebAPI.Managers.Models.KS.preparePayloadData(l);
            return payload;
        }

        public static WebAPI.Managers.Models.KS.KSData ExtractKSPayload(KS ks)
        {
            if (ks == null)
            {
                return new KS.KSData();
            }

            var pl = WebAPI.Managers.Models.KS.ExtractPayloadData(ks.Data);
            string udid = "";
            int createDate = 0;
            var udidRes = pl.Where(x => x.Key == PAYLOAD_UDID).FirstOrDefault();

            if (udidRes.Key != null)
            {
                udid = udidRes.Value;
            }

            var createDateTimeStr = pl.Where(x => x.Key == PAYLOAD_CREATE_DATE).FirstOrDefault();
            if (createDateTimeStr.Key != null)
            {
                int.TryParse(createDateTimeStr.Value, out createDate);
            }

            return new WebAPI.Managers.Models.KS.KSData() { UDID = udid, CreateDate = createDate };
        }

        internal static WebAPI.Managers.Models.KS.KSData ExtractKSPayload()
        {
            return ExtractKSPayload(KS.GetFromRequest());
        }
    }
}