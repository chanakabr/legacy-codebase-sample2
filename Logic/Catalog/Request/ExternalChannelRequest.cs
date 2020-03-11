using Core.Catalog.Response;
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

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class ExternalChannelRequest : BaseChannelRequest
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string deviceId;

        [DataMember]
        public string deviceType;

        [DataMember]
        public string utcOffset;

        [DataMember]
        public string free;

        #endregion

        #region Ctor

        public ExternalChannelRequest()
            : base()
        {
        }

        public ExternalChannelRequest(string channelId, string externalIdentifier, int groupID,
            int pageSize, int pageIndex, string userIP, string signature, string signString, Filter filter, string deviceId, string deviceType, string filterQuery = "")
            : base(groupID, pageSize, pageIndex, userIP, signature, signString, filter, filterQuery, channelId, externalIdentifier)
        {
            this.deviceId = deviceId;
            this.deviceType = deviceType;
        }

        #endregion

        #region Override Methods

        public override BaseResponse GetResponse(BaseRequest baseRequest)
        {
            UnifiedSearchExternalResponse response = new UnifiedSearchExternalResponse();

            try
            {
                BaseChannelRequest request = baseRequest as BaseChannelRequest;

                if (request == null)
                {
                    throw new ArgumentNullException("request object is null or required variable is null");
                }

                if (request.m_nGroupID == 0)
                {
                    response.status.Code = (int)eResponseStatus.BadSearchRequest;
                    response.status.Message = "No group Id was sent in request";

                    return response;
                }

                CheckSignature(baseRequest);

                int totalItems;
                List<UnifiedSearchResult> searchResults = new List<UnifiedSearchResult>();

                response.status = this.GetAssets(request, out totalItems, out searchResults, out response.requestId);

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
                        response.status.Message = "Data is not indexed for this group";
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Got error with Elasticsearch";
                    }
                }
                else if (ex is UriFormatException)
                {
                    response.status.Code = (int)eResponseStatus.AdapterUrlRequired;
                    response.status.Message = "Invalid adapter URL was defined. Correct adapter URL is required";
                }
                else if (ex is KalturaException)
                {
                    // This is a specific exception we created.
                    // If this specific KalturaException has StatusCode in its data, use it instead of the general code
                    if (ex.Data.Contains("StatusCode"))
                    {
                        response.status.Code = (int)ex.Data["StatusCode"];
                        response.status.Message = ex.Message;
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Failed getting assets of channel";
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
                        response.status.Message = "Failed getting assets of channel";
                    }
                }
                else
                {
                    response.status.Code = (int)eResponseStatus.Error;
                    response.status.Message = "Failed getting assets of channel";
                }
            }

            return response;
        }

        protected virtual Status GetAssets(BaseChannelRequest request, out int totalItems, out List<UnifiedSearchResult> searchResults, out string requestId)
        {
            ExternalChannelRequest externalRequest = request as ExternalChannelRequest;

            if (externalRequest == null)
            {
                externalRequest = new ExternalChannelRequest(this.internalChannelID, this.externalChannelID, this.m_nGroupID, this.m_nPageSize, this.m_nPageIndex,
                    this.m_sUserIP, this.m_sSignature, this.m_sSignString, this.m_oFilter, this.deviceId, this.deviceType);
            }

            return CatalogLogic.GetExternalChannelAssets(externalRequest, out totalItems, out searchResults, out requestId);
        }

        #endregion

    }
}
