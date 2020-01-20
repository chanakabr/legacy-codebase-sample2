using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSWrapper
{
    public class KSData
    {
        private const string PAYLOAD_UDID = "UDID";
        private const string PAYLOAD_CREATE_DATE = "CreateDate";
        private const string PAYLOAD_REGION = "r";
        private const string PAYLOAD_USER_SEGMENTS = "us";
        private const string PAYLOAD_USER_ROLES = "ur";
        private const string PAYLOAD_SIGNATURE = "sig";
        internal const string SignatureFormat = "{0}:{1}";

        public string UDID { get; private set; }
        public int CreateDate { get; private set; }
        public int RegionId { get; private set; }
        public List<long> UserSegments { get; private set; }
        public List<long> UserRoles { get; private set; }
        public string Signature { get; internal set; }

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
