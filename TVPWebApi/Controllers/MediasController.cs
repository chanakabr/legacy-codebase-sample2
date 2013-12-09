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
using TVPWebApi.Models.Objects;

namespace TVPWebApi.Controllers
{
    public class MediasController : ApiController
    {
        private readonly IMediasService _service;

        public MediasController(IMediasService service)
        {
            _service = service;
        }

        /// <summary>
        /// GetMediaInfo
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="media_type">Media type</param>
        /// <param name="pic_size">Required pic size</param>
        /// <param name="with_dynamic">Include dynamic data?</param>
        /// <param name="fields">Required fields</param>
        /// <returns></returns>
        [PartialResponseAttribute]
        public HttpResponseMessage Get(long media_id, int media_type = 0, string pic_size = "", bool with_dynamic = false, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetMediaInfo(initObj, media_id, media_type, pic_size, with_dynamic);
            
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get Media Comments
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="limit">Records per page</param>
        /// <param name="offset">Page index</param>
        /// <param name="fields">Required fields</param>
        /// <returns></returns>
        [PartialResponseAttribute]
        [ActionName("comments")]
        public HttpResponseMessage GetComment(int media_id, int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetMediaComments(initObj, media_id, limit, offset);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Add Comment
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [ActionName("comments")]
        public HttpResponseMessage PostComment(int media_id, CommentRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.AddComment(initObj, media_id, request_data.media_type, request_data.writer, request_data.header, request_data.sub_header, request_data.content, request_data.is_auto_active);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Rate Media
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [ActionName("rating")]
        public HttpResponseMessage PostRating(int media_id, RatingRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.RateMedia(initObj, media_id, request_data.media_type, request_data.extra_val);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get Media Mark
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <returns></returns>
        [ActionName("mediamark")]
        public HttpResponseMessage GetMediaMark(int media_id)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetMediaMark(initObj, media_id);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Report Media Mark
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [ActionName("mediamark")]
        public HttpResponseMessage PostMediaMark(long media_id, MediaMarkRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.MediaMark(initObj, request_data.action, request_data.media_type, media_id, request_data.file_id, request_data.location);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Report Media Hit
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="request_data">Additional data</param>
        /// <returns></returns>
        [ActionName("mediahit")]
        public HttpResponseMessage PostMediaHit(long media_id, MediaHitRequest request_data)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.MediaHit(initObj, request_data.media_type, media_id, request_data.file_id, request_data.location);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get related medias
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="media_type">Media type</param>
        /// <param name="pic_size">Pic size</param>
        /// <param name="limit">Records per page</param>
        /// <param name="offset">Page index</param>
        /// <param name="fields">Required fields</param>
        /// <returns></returns>
        [PartialResponseAttribute]
        [ActionName("related")]
        public HttpResponseMessage GetRelatedMedias(int media_id, int media_type = 0, string pic_size = "", int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetRelatedMedias(initObj, media_id, media_type, pic_size, limit, offset);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get people who watched
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <param name="media_type">Media type</param>
        /// <param name="pic_size">Pic size</param>
        /// <param name="limit">Records per page</param>
        /// <param name="offset">Page index</param>
        /// <param name="fields">Required fields</param>
        /// <returns></returns>
        [PartialResponseAttribute]
        [ActionName("peoplewhowatched")]
        public HttpResponseMessage GetPeopleWhoWatched(int media_id, int media_type = 0, string pic_size = "", int limit = 10, int offset = 0, string fields = "")
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetPeopleWhoWatched(initObj, media_id, media_type, pic_size, limit, offset);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Check Geo block
        /// </summary>
        /// <param name="media_id">Media id</param>
        /// <returns></returns>
        [ActionName("geoblock")]
        public HttpResponseMessage GetGeoBlock(int media_id)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.CheckGeoBlockForMedia(initObj, media_id);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Get license link
        /// </summary>
        /// <param name="file_id">File id</param>
        /// <param name="base_link">Base link</param>
        /// <returns></returns>
        [ActionName("licenselink")]
        public HttpResponseMessage GetMediaLicenseLink(int file_id, string base_link)
        {
            InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

            var response = _service.GetMediaLicenseLink(initObj, file_id, base_link);

            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        //[ActionName("licenselink")]
        //public HttpResponseMessage GetIsItemPurchased(int file_id, string site_guid)
        //{
        //    InitializationObject initObj = (InitializationObject)Request.Properties["InitObj"];

        //    var response = _service.IsItemPurchased(initObj, file_id, site_guid);

        //    if (response == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, response);
        //}
    }
}
