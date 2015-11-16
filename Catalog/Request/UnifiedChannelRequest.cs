using ApiObjects.Response;
using Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace Catalog.Request
{
    [Serializable]
    [DataContract]
    public class UnifiedChannelRequest : BaseRequest, IRequestImp
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string channelID;

        [DataMember]
        public eChannelType type;

        #endregion

        #region Ctor

        public UnifiedChannelRequest(int groupID, int pageSize, int pageIndex, string userIP, string signature, string signString, Filter filter, string channelId = "")
            : base(pageSize, pageIndex, userIP, groupID, filter, signature, signString)
        {
            this.channelID = channelId;
        }

        #endregion

        #region IRequestImp Members

        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            UnifiedSearchResponse response = new UnifiedSearchResponse();

            try
            {
                UnifiedChannelRequest request = baseRequest as UnifiedChannelRequest;

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

                response.status = this.GetAssets(request, out totalItems, out searchResults);

                //response.status = Catalog.GetExternalChannelAssets(request, out totalItems, out searchResults);

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

        protected virtual Status GetAssets(UnifiedChannelRequest request, out int totalItems, out List<UnifiedSearchResult> searchResults)
        {
            totalItems = 0;
            searchResults = null;
            return new Status()
            {
                Code = (int)eResponseStatus.Error,
                Message = "Web service failure"
            };
        }

        #endregion
    }

    public enum eChannelType
    {
        Internal,
        External
    }
}
