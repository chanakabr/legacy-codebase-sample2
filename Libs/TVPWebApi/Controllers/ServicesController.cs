using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Http;
using TVPApi;
using TVPWebApi.Models;

namespace TVPWebApi.Controllers
{
    public class ServicesController : ApiController
    {

        /// <summary>
        /// Generate token based on Initialization Object.
        /// </summary>
        /// <param name="initObj">Initialization Object</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GenerateToken(InitializationObject initObj)
        {
            if (initObj == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            string _token = string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, initObj);
                
                _token = Convert.ToBase64String(ms.ToArray());
            }

            return Request.CreateResponse(HttpStatusCode.OK, _token);
        }
    }
}
