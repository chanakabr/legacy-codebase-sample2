using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace WebAPI
{
    /// <summary>
    /// Summary description for Swagger
    /// </summary>
    public class Swagger : IHttpHandler
    {

        public async void ProcessRequest(HttpContext context)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(context.Request.Url.AbsoluteUri.Replace("swagger.ashx", "docs/v1/swagger"));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                HttpResponseMessage response = await client.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    json = json.Replace("_service/", "api/service/");//.Replace("\"query\"", "\"body\"");

                    context.Response.ContentType = "application/json";
                    context.Response.Write(json);
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}