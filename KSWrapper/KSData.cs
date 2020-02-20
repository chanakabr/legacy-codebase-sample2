using System.Collections.Generic;
using System.Linq;
using TVinciShared;

namespace KSWrapper
{
    public sealed class KSData
    {
        private const string PAYLOAD_UDID = "UDID";
        private const string PAYLOAD_CREATE_DATE = "CreateDate";
        private const string PAYLOAD_REGION = "r";
        private const string PAYLOAD_USER_SEGMENTS = "us";
        private const string PAYLOAD_USER_ROLES = "ur";
        private const string PAYLOAD_SIGNATURE = "sig";
        public const string SIGNATURE_FORMAT = "{0}:{1}";

        public string UDID { get; private set; }
        public int CreateDate { get; private set; }
        public int RegionId { get; private set; }
        public List<long> UserSegments { get; private set; }
        public List<long> UserRoles { get; private set; }
        public string Signature { get; internal set; }

        public KSData()
        {
        }

        public KSData(Dictionary<string, string> payloadData)
        {
            if (payloadData.ContainsKey(PAYLOAD_UDID))
            {
                this.UDID = payloadData[PAYLOAD_UDID];
            }

            if (payloadData.ContainsKey(PAYLOAD_CREATE_DATE))
            {
                int createDate = 0;
                int.TryParse(payloadData[PAYLOAD_CREATE_DATE], out createDate);
                this.CreateDate = createDate;
            }

            if (payloadData.ContainsKey(PAYLOAD_REGION))
            {
                int regionId = 0;
                int.TryParse(payloadData[PAYLOAD_REGION], out regionId);
                this.RegionId = regionId;
            }

            this.UserSegments = new List<long>();
            if (payloadData.ContainsKey(PAYLOAD_USER_SEGMENTS))
            {
                this.UserSegments.AddRange(payloadData[PAYLOAD_USER_SEGMENTS].GetItemsIn<List<long>, long>());
            }

            this.UserRoles = new List<long>();
            if (payloadData.ContainsKey(PAYLOAD_USER_ROLES))
            {
                this.UserRoles.AddRange(payloadData[PAYLOAD_USER_ROLES].GetItemsIn<List<long>, long>());
            }

            if (payloadData.ContainsKey(PAYLOAD_SIGNATURE))
            {
                this.Signature = payloadData[PAYLOAD_SIGNATURE];
            }
        }

        public KSData(KSData payload, int createDate)
        {
            this.CreateDate = createDate;
            this.UDID = payload.UDID;
            this.RegionId = payload.RegionId;
            this.UserSegments = payload.UserSegments;
            this.UserRoles = payload.UserRoles;
            this.Signature = this.Signature;
        }

        public KSData(string udid, int createDate, int regionId, List<long> userSegments, List<long> userRoles, string signature = "")
        {
            this.UDID = udid;
            this.CreateDate = createDate;
            this.RegionId = regionId;
            this.UserSegments = userSegments;
            this.UserRoles = userRoles;
            this.Signature = signature;
        }

        public string PrepareKSPayload()
        {
            var ksDataList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(PAYLOAD_UDID, this.UDID),
                new KeyValuePair<string, string>(PAYLOAD_CREATE_DATE, this.CreateDate.ToString()),
                new KeyValuePair<string, string>(PAYLOAD_REGION, this.RegionId.ToString())
            };

            if (this.UserSegments != null && this.UserSegments.Count > 0)
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_USER_SEGMENTS, string.Join(",", this.UserSegments.OrderBy(x => x))));
            }

            if (this.UserRoles != null && this.UserRoles.Count > 0)
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_USER_ROLES, string.Join(",", this.UserRoles.OrderBy(x => x))));
            }

            if (!string.IsNullOrEmpty(this.Signature))
            {
                ksDataList.Add(new KeyValuePair<string, string>(PAYLOAD_SIGNATURE, this.Signature));
            }
            
            var payload = string.Join(";;", ksDataList.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
            return payload;
        }
    }
}
