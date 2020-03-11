using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TVinciShared;

namespace NPVR
{
    internal class HttpUtils
    {
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient(ApplicationConfiguration.Current.NPVRHttpClientConfiguration);

        public static bool TrySendHttpGetRequest(string url, Encoding encoding, ref int responseStatus, ref string result, ref string errorMsg, Dictionary<string, string> headersToAdd = null)
        {
            bool success = false;

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                if (headersToAdd != null)
                {
                    foreach (KeyValuePair<string, string> pair in headersToAdd)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
                        {
                            request.Headers.Add(pair.Key, pair.Value);
                        }
                    }
                }

                using (var response = httpClient.SendAsync(request).ExecuteAndWait())
                {
                    result = response.Content.ReadAsStringAsync().ExecuteAndWait();
                    responseStatus = (int)response.StatusCode;

                    response.EnsureSuccessStatusCode();

                    success = true;
                }
            }
            catch (Exception ex)
            {
                errorMsg = String.Concat(errorMsg, " || Ex Msg: ", ex.Message, " || Ex Type: ", ex.GetType().Name, " || ST: ", ex.StackTrace, " || ");
                success = false;
            }

            return success;
        }

    }
}
