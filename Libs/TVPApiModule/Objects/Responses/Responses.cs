using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiModule.Objects.Responses
{
    public class ServicesResponse
    {
        [JsonProperty(PropertyName = "services")]
        public ServiceObject[] Services { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public ServicesResponse()
        {
            Status = new Status();
        }
    }

    public class LicensedLinkResponse
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public LicensedLinkResponse()
        {
            Status = new Status();
        }

        public LicensedLinkResponse(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LicensedLinkResponse response) 
        {
            Url = response.mainUrl;
            Status = new Status();
            Status.Message = response.status;

            switch (response.status)
            {
                case "OK":
                    Status.Code = 0;
                    break;
                case "Error":
                    Status.Code = 1;
                    break;
                case "Unknown":
                    Status.Code = 2;
                    break;
                case "InvalidInput":
                    Status.Code = 3;
                    break;
                case "InvalidDevice":
                    Status.Code = 4;
                    break;
                case "InvalidPrice":
                    Status.Code = 5;
                    break;
                case "Concurrency":
                    Status.Code = 6;
                    break;
                case "MediaConcurrency":
                    Status.Code = 7;
                    break;
                case "InvalidBaseLink":
                    Status.Code = 8;
                    break;
                case "InvalidFileData":
                    Status.Code = 9;
                    break;
                case "UserSuspended":
                    Status.Code = 10;
                    break;
                case "ServiceNotAllowed":
                    Status.Code = 11;
                    break;
                default:
                    break;
            }
        }
    }

    public class DomainLimitationModuleResponse
    {
        [JsonProperty(PropertyName = "domain_limitation_module")]
        public DLMResponse DLM { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public DomainLimitationModuleResponse()
        {
            Status = new Status();
        }

        public DomainLimitationModuleResponse(DLMResponse dlm)
        {
            DLM = dlm;
            Status = new Status();
            Status.Code = 0;
            Status.Message = "OK";
        }
    }
}
