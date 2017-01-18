using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using TVinciShared;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Request
{
    /**************************************************************************
    *Get People Who Watched Also Watched
     *Get SiteGuid, mediaId, countryId
     *and return all the :
     * The Top 8 most viewed medias that have been watched by the 
     * top 30 people that watched that mediaId (that was send by parameter)
     * ***********************************************************************/
    [DataContract]
    public class PWWAWProtocolRequest : BaseProtocolRequest
    {       

        [DataMember]
        public Int32 m_nMediaID;
        [DataMember]
        public Int32 m_nCountryID;


        public PWWAWProtocolRequest()
            : base()
        {
        }
        public PWWAWProtocolRequest(PWWAWProtocolRequest p)
            : base(p.m_nPageSize, p.m_nPageIndex, p.m_sUserIP, p.m_nGroupID, p.m_oFilter, p.m_sSignature, p.m_sSignString, p.m_sSiteGuid, p.domainId)
        {
            m_nMediaID = p.m_nMediaID;
            m_sSiteGuid = p.m_sSiteGuid;
            m_nCountryID = p.m_nCountryID;
        }

        protected override int GetProtocolMaxResultsSize()
        {
            int res = 0;
            string resultsSize = Utils.GetWSURL("PWWAWP_MAX_RESULTS_SIZE");
            if (resultsSize.Length > 0 && Int32.TryParse(resultsSize, out res))
                return res;
            return CatalogLogic.DEFAULT_PWWAWP_MAX_RESULTS_SIZE;
        }

        protected override List<SearchResult> ExecuteNonIPNOProtocol(BaseRequest oBaseRequest)
        {
            int nGroupID = 0, nMediaID = 0, nCountryID = 0;
            string sSiteGuid = string.Empty;
            GetEndDateLanguageDeviceID(oBaseRequest);
            GetProtocolData(oBaseRequest, ref nGroupID, ref nMediaID, ref sSiteGuid, ref nCountryID);
            DataTable dt = CatalogDAL.Get_PWWAWProtocol(nGroupID, nMediaID, sSiteGuid, nCountryID, nLanguageID, sEndDate, nDeviceID);
            return ConvertProtocolDataTableToList(dt, "m_id");
        }

        protected override List<SearchResult> ExecuteIPNOProtocol(BaseRequest oBaseRequest, int nOperatorID, List<List<string>> jsonizedChannelsDefinitions,
            ref ISearcher initializedSearcher)
        {
            int nGroupID = 0, nMediaID = 0, nCountryID = 0;
            string sSiteGuid = string.Empty;
            GetEndDateLanguageDeviceID(oBaseRequest);
            GetProtocolData(oBaseRequest, ref nGroupID, ref nMediaID, ref sSiteGuid, ref nCountryID);
            DataTable dt = CatalogDAL.Get_IPWWAWProtocol(nGroupID, nMediaID, sSiteGuid,
                nCountryID, nLanguageID, sEndDate, nDeviceID, nOperatorID);
            List<SearchResult> initialResults = ConvertProtocolDataTableToList(dt, "m_id");

            return GetProtocolFinalResultsUsingSearcher(initialResults, ref initializedSearcher, jsonizedChannelsDefinitions, nGroupID);
        }

        private void GetProtocolData(BaseRequest oRequest, ref int nGroupID, ref int nMediaID, ref string sSiteGuid, ref int nCountryID)
        {
            PWWAWProtocolRequest request = oRequest as PWWAWProtocolRequest;
            if (request == null)
                throw new ArgumentNullException("Request object is null");
            nGroupID = request.m_nGroupID;
            nMediaID = request.m_nMediaID;
            sSiteGuid = request.m_sSiteGuid;
            nCountryID = request.m_nCountryID;
        }
    }
}
