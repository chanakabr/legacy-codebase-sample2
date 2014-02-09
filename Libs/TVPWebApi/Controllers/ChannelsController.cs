using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using TVPApi;
using TVPWebApi.Models;

namespace TVPWebApi.Controllers
{
    public class ChannelsController : ApiController
    {
        private readonly IChannelsService _service;

        public ChannelsController(IChannelsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Channel Media List
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="channel_id">Channel id</param>
        /// <param name="pic_size">Pic size</param>
        /// <param name="order_by">Order by</param>
        /// <param name="limit">Records per page</param>
        /// <param name="offset">Page index</param>
        /// <param name="fields">Required fields</param>
        /// <returns></returns>
        [PartialResponseAttribute]
        public HttpResponseMessage GetChannelMediaList(long channel_id, string pic_size, TVPApi.OrderBy order_by = OrderBy.None, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetChannelMediaList(initObj, channel_id, pic_size, limit, offset, order_by);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
