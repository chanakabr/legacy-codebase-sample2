using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TVPApiModule.HttpHandlers
{
    public class CrossOriginHandler : IHttpHandler
    {
        #region IHttpHandler Members
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //Clear the response (just in case)
            ClearResponse(context);

            //Checking the method
            switch (context.Request.HttpMethod.ToUpper())
            {
                //Cross-Origin preflight request
                case "OPTIONS":
                    //Set allowed method and headers
                    SetAllowCrossSiteRequestHeaders(context);
                    //Set allowed origin
                    SetAllowCrossSiteRequestOrigin(context);
                    
                    break;
                //Cross-Origin actual or simple request
                case "POST":
                case "GET":
                    //Disable caching
                    //SetNoCacheHeaders(context);
                    //Set allowed origin
                    SetAllowCrossSiteRequestOrigin(context);
                    //Generate response
                    //context.Response.ContentType = "text/plain";
                    context.Response.ContentEncoding = Encoding.UTF8;
                    //context.Response.Write("<h1>Hello World! [powered by Cross-Origin Resource Sharing]</h1>");
                    break;
                //We doesn't support any other methods than OPTIONS and GET
                default:
                    context.Response.Headers.Add("Allow", "OPTIONS, GET");
                    context.Response.StatusCode = 405;
                    break;
            }
        }
        #endregion

        #region Methods
        protected void ClearResponse(HttpContext context)
        {
            context.Response.ClearHeaders();
            context.Response.ClearContent();
            context.Response.Clear();
        }

        protected void SetNoCacheHeaders(HttpContext context)
        {
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            context.Response.Cache.SetValidUntilExpires(false);
            context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
        }

        private void SetAllowCrossSiteRequestHeaders(HttpContext context)
        {
            ////We allow only GET method
            //string requestMethod = context.Request.Headers["Access-Control-Request-Method"];
            //if (!String.IsNullOrEmpty(requestMethod) && requestMethod.ToUpper() == "GET")
            //    context.Response.AppendHeader("Access-Control-Allow-Methods", "GET");

            ////We allow any custom headers
            //string requestHeaders = context.Request.Headers["Access-Control-Request-Headers"];
            //if (!String.IsNullOrEmpty(requestHeaders))
                context.Response.AppendHeader("Access-Control-Allow-Headers", "origin, x-requested-with, content-type, accept");
        }

        private void SetAllowCrossSiteRequestOrigin(HttpContext context)
        {
            //string origin = context.Request.Headers["Origin"];
            //if (!String.IsNullOrEmpty(origin))
            //    //You can make some sophisticated checks here
            //    context.Response.AppendHeader("Access-Control-Allow-Origin", origin);
            //else
                //This is necessary for Chrome/Safari actual request
                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
        }
        #endregion
    }
}
