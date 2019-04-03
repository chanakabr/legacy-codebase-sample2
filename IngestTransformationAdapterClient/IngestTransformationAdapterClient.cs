using System;
using System.Collections.Generic;
using ApiObjects;
using RestSharp;
using System.Linq;
using System.Threading.Tasks;
using RestAdaptersCommon;
using Newtonsoft.Json;
using KLogMonitor;
using System.Reflection;

namespace AdapterClients.IngestTransformation
{
    public class IngestTransformationAdapterClient
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IngestProfile _IngestProfile;
        private RestClient _Client;

        public IngestTransformationAdapterClient(IngestProfile profile)
        {
            _IngestProfile = profile;
            _Client = new RestClient(profile.TransformationAdapterUrl);
        }

        public ApiObjects.AdapterStatus SetConfiguration()
        {
            return Task.Run(() => SetConfigurationAsync()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<ApiObjects.AdapterStatus> SetConfigurationAsync()
        {
            var data = _IngestProfile.Settings.ToDictionary(k => k.Key, v => v.Value);
            var request = new RestRequest("SetConfiguration", Method.POST);
            request.AddJsonBody(data);

            var response = await _Client.MakeRequestAsync(request);

            var responseObj = JsonConvert.DeserializeObject<AdapterStatus>(response.Content);
            return responseObj == null? ApiObjects.AdapterStatus.Error: (ApiObjects.AdapterStatus)responseObj.Code;
        }

        public string Transform(string fileUrl)
        {
            return Task.Run(() => TransformAsync(fileUrl)).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> TransformAsync(string fileUrl)
        {
            var data = new TransformRequest { FileUrl = fileUrl };
            var request = new RestRequest("Transform", Method.POST);
            request.AddJsonBody(data);

            // TODO: Arthur think about the logging of response  when the result here is the full xmlTv data :\
            var response = await _Client.MakeRequestAsync(request);

            return response?.Content;
        }

        private class TransformRequest
        {
            public string FileUrl { get; set; }
        }

        private class AdapterStatus
        {
            public ApiObjects.AdapterStatus Code { get; set; }

            public string Message { get; set; }

            public AdapterStatus()
            {
            }
        }
    }




}
