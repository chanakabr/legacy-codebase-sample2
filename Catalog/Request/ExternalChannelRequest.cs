using Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.SearchObjects;
using ApiObjects.Response;
using System.Web;

namespace Catalog.Request
{
    [Serializable]
    [DataContract]
    public class ExternalChannelRequest : BaseRequest, IRequestImp
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string externalChannelId;

        [DataMember]
        public string deviceId;

        [DataMember]
        public string deviceType;

        [DataMember]
        public string utcOffset;

        #endregion

        #region Ctor

        public ExternalChannelRequest(string externalChannelId, int groupID, 
            int pageSize, int pageIndex, string userIP, string signature, string signString, Filter filter, string deviceId, string deviceType)
            : base(pageSize, pageIndex, userIP, groupID, filter, signature, signString)
        {
            this.externalChannelId = externalChannelId;
            this.deviceId = deviceId;
        }

        #endregion

        #region IRequestImp Members

        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            UnifiedSearchResponse response = new UnifiedSearchResponse();

            try
            {
                ExternalChannelRequest request = baseRequest as ExternalChannelRequest;

                if (request == null)
                {
                    throw new ArgumentNullException("request object is null or required variable is null");
                }

                if (request.m_nGroupID == 0)
                {
                    var exception = new ArgumentException("No group Id was sent in request");
                    exception.Data["StatusCode"] = (int)eResponseStatus.BadSearchRequest;

                    throw exception;
                }

                CheckSignature(baseRequest);

                int totalItems;
                List<UnifiedSearchResult> searchResults = new List<UnifiedSearchResult>();

                response.status = Catalog.GetExternalChannelAssets(request, out totalItems, out searchResults);

                response.searchResults = searchResults;
                response.m_nTotalItems = totalItems;
            }
            catch (Exception ex)
            {
                log.Error("Error - GetResponse - " +
                    string.Format("Exception: group = {0} siteGuid = {1} message = {2}, ST = {3}",
                    baseRequest.m_nGroupID, // {0}
                    baseRequest.m_sSiteGuid, // {1}
                    ex.Message, // {2}
                    ex.StackTrace // {3}
                    ), ex);

                if (ex is HttpException)
                {
                    if ((ex as HttpException).GetHttpCode() == 404)
                    {
                        response.status.Code = (int)eResponseStatus.IndexMissing;
                        response.status.Message = "Data not index for this group";
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Got error with Elasticsearch";
                    }
                }
                else if (ex is ArgumentException)
                {
                    // This is a specific exception we created.
                    // If this specific ArgumentException has StatusCode in its data, use it instead of the general code
                    if (ex.Data.Contains("StatusCode"))
                    {
                        response.status.Code = (int)ex.Data["StatusCode"];
                        response.status.Message = ex.Message;
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Search failed";
                    }
                }
                else
                {
                    response.status.Code = (int)eResponseStatus.Error;
                    response.status.Message = "Search failed";
                }
            }

            return response;
        }

        #endregion
    }
}
