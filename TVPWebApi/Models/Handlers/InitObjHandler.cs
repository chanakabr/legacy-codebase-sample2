using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TVPApi;

namespace TVPWebApi.Models
{
    public class InitObjHandler : DelegatingHandler
    {
        const string _header = "X-InitObj";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains(_header))
            {
                InitializationObject obj = null;

                try
                {
                    string sInitObj = request.Headers.GetValues(_header).FirstOrDefault();

                    byte[] bytes = Convert.FromBase64String(sInitObj);

                    using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
                    {
                        ms.Write(bytes, 0, bytes.Length);

                        ms.Position = 0;

                        obj = (InitializationObject)new BinaryFormatter().Deserialize(ms);
                    }
                }
                catch (Exception)
                {
                    var tsc = new TaskCompletionSource<HttpResponseMessage>();

                    tsc.SetResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Token invalid.")
                    });

                    return tsc.Task;
                }

                request.Properties["InitObj"] = obj;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}