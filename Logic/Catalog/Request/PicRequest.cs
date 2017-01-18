using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using Core.Catalog.Cache;
using GroupsCacheManager;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [DataContract]
    public class PicRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public List<int> m_nPicIds;
       
        public PicRequest() : base()
        {

        }

        public PicRequest(List<int> nPicIds, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nPicIds = nPicIds;
        }


        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                PicRequest request = (PicRequest)oBaseRequest;
                PicResponse response = new PicResponse();
                PicObj oPicObj = new PicObj();
                Picture oPicture = new Picture();
                              
                if (request == null )
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                List<int> lSubGroup = groupManager.GetSubGroup(nParentGroupID);

                DataTable dt = CatalogDAL.Get_PicProtocol(request.m_nGroupID, request.m_nPicIds, lSubGroup);

                if (dt != null)
                {
                    if (dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        response.m_lObj = new List<BaseObject>();
                        response.m_nTotalItems = dt.Rows.Count;
                        int prevID = -1;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {   
                            if (prevID != Utils.GetIntSafeVal(dt.Rows[i],"id"))
                            {
                                //save current picID as prev
                                prevID = Utils.GetIntSafeVal(dt.Rows[i],"id");
                                //add PicObj to response
                                if (i>0)
                                    response.m_lObj.Add(oPicObj);

                                //create "clean" , new object 
                                oPicObj = new PicObj();
                                oPicObj.AssetId = prevID.ToString(); 
                            }
                            //same pic id , just adding url, size
                            oPicture = new Picture();
                            oPicture.m_sSize = Utils.GetStrSafeVal(dt.Rows[i],"PicSize");
                            oPicture.m_sURL = Utils.GetStrSafeVal(dt.Rows[i],"m_sURL");
                            oPicObj.m_Picture.Add(oPicture);
                        }
                        response.m_lObj.Add(oPicObj); //last pic id
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
