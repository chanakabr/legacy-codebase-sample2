using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    public class KSUtils
    {
        public const string PAYLOAD_UDID = "UDID";

        public static string PrepareKSPayload(WebAPI.Managers.Models.KS.KSData pl)
        {
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(PAYLOAD_UDID, pl.UDID));
            string payload = WebAPI.Managers.Models.KS.preparePayloadData(l);
            return payload;
        }

        public static WebAPI.Managers.Models.KS.KSData ExtractKSPayload(KS ks)
        {
            var pl = WebAPI.Managers.Models.KS.ExtractPayloadData(ks.Data);
            string udid = "";
            var udidRes = pl.Where(x => x.Key == PAYLOAD_UDID).FirstOrDefault();

            if (udidRes.Key != null)
                udid = udidRes.Value;

            return new WebAPI.Managers.Models.KS.KSData() { UDID = udid };
        }

        internal static WebAPI.Managers.Models.KS.KSData ExtractKSPayload()
        {
            return ExtractKSPayload(KS.GetFromRequest());
        }
    }
}