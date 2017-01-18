using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TVinciShared;
using System.Runtime.Serialization;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Request
{
    /**************************************************************************
    *Get People Who Like Also Like
     *Get SiteGuid, mediaId, socialAction (default value = 1) , socialPlatform 
     *(default value = 1), mediaFileID , countryId
     *and return all the :
     * The Top 8 most viewed medias that have been liked by the 
     * top 30 people that watched that mediaId  (that was send by parameter)
     * and liked it
     * ***********************************************************************/
    [DataContract]
    public class PWLALProtocolRequest : BaseProtocolRequest
    {        

        [DataMember]
        public int m_nMediaID;
        [DataMember]
        public int m_nSocialAction;
        [DataMember]
        public int m_nSocialPlatform;
        [DataMember]
        public int m_nMediaFileID;
        [DataMember]
        public int m_nCountryID;

        public PWLALProtocolRequest()
            : base()
        {
            m_nSocialAction = 1;
            m_nSocialPlatform = 1;
        }

        public PWLALProtocolRequest(int nMediaID, int nSocialAction, int nSocialPlatform, int nMediaFileID, 
            int nCountryID, string sSiteGuide, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, 
            string sUserIP, Filter oFilter, string sSignature, string sSignString, int nDomainId)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString, sSiteGuide, nDomainId)
        {
            m_sSiteGuid = sSiteGuide;
            m_nMediaID = nMediaID;
            m_nSocialAction = nSocialAction;
            m_nSocialPlatform = nSocialPlatform;
            m_nMediaFileID = nMediaFileID;
            m_nCountryID = nCountryID;
        }

        protected override int GetProtocolMaxResultsSize()
        {
            int res = 0;
            string resultsSize = Utils.GetWSURL("PWLALP_MAX_RESULTS_SIZE");
            if (resultsSize.Length > 0 && Int32.TryParse(resultsSize, out res))
                return res;
            return CatalogLogic.DEFAULT_PWLALP_MAX_RESULTS_SIZE;
        }

        protected override List<SearchResult> ExecuteNonIPNOProtocol(BaseRequest oRequest)
        {
            GetEndDateLanguageDeviceID(oRequest);
            DataTable dt = CatalogDAL.Get_PWLALProtocol(m_nGroupID, m_nMediaID, m_sSiteGuid, m_nSocialAction,
                m_nSocialPlatform, m_nMediaFileID,
                m_nCountryID, nLanguageID, sEndDate, nDeviceID);
            return ConvertProtocolDataTableToList(dt, "id");
        }

        protected override List<SearchResult> ExecuteIPNOProtocol(BaseRequest oRequest, int nOperatorID, List<List<string>> jsonizedChannelsDefinitions, ref ISearcher initializedSearcher)
        {
            int nGroupID = 0, nMediaID = 0, nSocialAction = 0, nSocialPlatform = 0, nMediaFileID = 0, nCountryID = 0;
            string sSiteGuid = string.Empty;
            GetEndDateLanguageDeviceID(oRequest);
            GetProtocolData(oRequest, ref nGroupID, ref m_nMediaID, ref sSiteGuid, ref nSocialAction, ref nSocialPlatform,
                ref nMediaFileID, ref nCountryID);
            DataTable dt = CatalogDAL.Get_IPWLALProtocol(nGroupID, nMediaID, sSiteGuid, nSocialAction,
                nSocialPlatform, nMediaFileID,
                nCountryID, nLanguageID, sEndDate, nDeviceID, nOperatorID);
            List<SearchResult> initialResults = ConvertProtocolDataTableToList(dt, "id");

            return GetProtocolFinalResultsUsingSearcher(initialResults, ref initializedSearcher, jsonizedChannelsDefinitions, m_nGroupID);
        }

        private void GetProtocolData(BaseRequest oRequest, ref int nGroupID, ref int nMediaID, ref string sSiteGuid,
            ref int nSocialAction, ref int nSocialPlatform, ref int nMediaFileID, ref int nCountryID)
        {
            PWLALProtocolRequest request = oRequest as PWLALProtocolRequest;
            if (request == null)
                throw new Exception("Request object is null");
            nGroupID = request.m_nGroupID;
            nMediaID = request.m_nMediaID;
            sSiteGuid = request.m_sSiteGuid;
            nSocialAction = request.m_nSocialAction;
            nSocialPlatform = request.m_nSocialPlatform;
            nMediaFileID = request.m_nMediaFileID;
            nCountryID = request.m_nCountryID;
        }

    }
}
