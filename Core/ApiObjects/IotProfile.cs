using ApiObjects.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class IotProfile : ICrudHandeledObject
    {
        public string AdapterUrl { get; set; }
        public IotProfileAws IotProfileAws { get; set; }
    }

    public class IotProfileAws : ICrudHandeledObject
    {
        public string IotEndPoint { get; set; }
        public string PfxPath { get; set; }
        public string PfxPassword { get; set; }
        public string CertificatePath { get; set; }
        public int BrokerPort { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string TTL { get; set; }
        public string IotPolicyName { get; set; }
        public string UserPoolId { get; set; }
        public string ClientId { get; set; }
        public string IdentityPoolId { get; set; }
        public string Region { get; set; }
        public long UpdateDate { get; set; }
    }
}
