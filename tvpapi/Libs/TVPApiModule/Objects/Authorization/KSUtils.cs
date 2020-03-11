using System.Collections.Generic;
using System.Linq;

namespace TVPApiModule.Objects.Authorization
{
    public class KSUtils
    {
        public const string PAYLOAD_UDID = "UDID";
        public const string PAYLOAD_CREATE_DATE = "CreateDate";

        public static string PrepareKSPayload(KS.KSData pl)
        {
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(PAYLOAD_UDID, pl.UDID));
            l.Add(new KeyValuePair<string, string>(PAYLOAD_CREATE_DATE, pl.CreateDate.ToString()));
            string payload = KS.preparePayloadData(l);
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

            return new KS.KSData() { UDID = udid, CreateDate = createDate };
        }


        /*
        internal static WebAPI.Managers.Models.KS.KSData ExtractKSPayload()
        {
            return ExtractKSPayload(KS.GetFromRequest());
        }
        */
    }
}