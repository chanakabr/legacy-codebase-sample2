using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using Tvinci.Core.DAL;
using Core.Catalog.Cache;
using GroupsCacheManager;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    /**************************************************************************
    * Get Channel List
    * return all the :
    * Channels related to a specific category id
    * ************************************************************************/
    [DataContract]
    public class ChannelsListRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());         
        
        [DataMember]
        public int m_nCategoryID;

        public ChannelsListRequest()
            : base()
        {
        }

        public ChannelsListRequest(Int32 nCategoryID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nCategoryID = nCategoryID;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                ChannelsListRequest request = (ChannelsListRequest)oBaseRequest;
                ChannelDetailsResponse response = new ChannelDetailsResponse();
                channelObj chObj;
                Picture pic;


                if (request == null || request.m_oFilter == null)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);
          
                
                //Get Channels For CategoryID

                int nLanguage = 0;
                if (request.m_oFilter != null)
                    nLanguage = request.m_oFilter.m_nLanguage;

                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                List<int> lSubGroupTree = groupManager.GetSubGroup(nParentGroupID);
                DataSet ds = CatalogDAL.Get_ChannelsListByCategory(request.m_nCategoryID, request.m_nGroupID, nLanguage, lSubGroupTree);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Columns != null)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            chObj = new channelObj();
                            chObj.m_nChannelID = Utils.GetIntSafeVal(ds.Tables[0].Rows[i],"id");
                            chObj.m_nGroupID = Utils.GetIntSafeVal(ds.Tables[0].Rows[i],"group_id");
                            chObj.m_sDescription = Utils.GetStrSafeVal(ds.Tables[0].Rows[i],"Description");
                            chObj.m_sTitle = Utils.GetStrSafeVal(ds.Tables[0].Rows[i],"title");
                            chObj.m_sEditorRemarks = Utils.GetStrSafeVal(ds.Tables[0].Rows[i],"EDITOR_REMARKS");
                            if (!string.IsNullOrEmpty(ds.Tables[0].Rows[i]["LINEAR_START_TIME"].ToString()))
                            {
                                chObj.m_dLinearStartTime = System.Convert.ToDateTime(ds.Tables[0].Rows[i]["LINEAR_START_TIME"].ToString());
                            }

                            if (ds.Tables[1].Columns != null)
                            {
                                DataRow[] dr = ds.Tables[1].Select("ID =" + chObj.m_nChannelID);
                               
                                for (int j = 0; j < dr.Count(); j++)
                                {
                                    pic = new Picture();                                    
                                    pic.m_sSize = Utils.GetStrSafeVal(dr[j],"PicSize");
                                    pic.m_sURL = Utils.GetStrSafeVal(dr[j],"m_sURL");
                                    chObj.m_lPic.Add(pic);
                                }
                            }   
                        
                            response.m_lchannelList.Add(chObj);
                        }
                        response.m_nTotalItems = response.m_lchannelList.Count;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
