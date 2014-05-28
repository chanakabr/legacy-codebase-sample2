using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Logger;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using Catalog.Cache;

namespace Catalog
{
    /**************************************************************************
    * Get Personal Last Device
    * Get SiteGuid + MediaID
    * and return  : the last devicename + date 
    * ************************************************************************/
    [DataContract]
    public class PersonalLasDeviceRequest : BaseRequest, IRequestImp
    {
         private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public List<int> m_nMediaIDs;          

        public PersonalLasDeviceRequest() : base()
        {
        }

        public PersonalLasDeviceRequest(PersonalLasDeviceRequest p)
            : base(p.m_nPageSize, p.m_nPageIndex, p.m_sUserIP, p.m_nGroupID, p.m_oFilter, p.m_sSignature, p.m_sSignString)
        {
            m_sSiteGuid = p.m_sSiteGuid;
            m_nMediaIDs = p.m_nMediaIDs;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                PersonalLasDeviceRequest request = (PersonalLasDeviceRequest)oBaseRequest;
                PersonalLastDeviceResponse response = new PersonalLastDeviceResponse();
                PersonalLastDevice oPersonalLastWatched;
                
                string xmlresult = "";
                xmlresult = SerializeToXML<PersonalLasDeviceRequest>(request);

                _logger.Info(xmlresult);
                _logger.Info(string.Format("{0}: {1}", "PersonalLasDevice Start At", DateTime.Now));

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                GroupManager groupManager = new GroupManager();
                List<int> lSubGroupTree = groupManager.GetSubGroup(request.m_nGroupID);
                DataTable dt = CatalogDAL.Get_PersonalLasDevice(request.m_nMediaIDs, request.m_nGroupID, request.m_sSiteGuid, lSubGroupTree);
                int startIndex = -1;
                int count = -1;

                if (dt != null)
                {
                    if (dt.Columns != null)
                    {
                        bool bContinue = Utils.GetPagingValues(dt.Rows.Count, request.m_nPageIndex, request.m_nPageSize, ref startIndex, ref count);
                        if (bContinue)
                        {
                            for (int i = startIndex; i < count; i++)
                            {
                                oPersonalLastWatched = new PersonalLastDevice();

                                oPersonalLastWatched.m_nID = Utils.GetIntSafeVal(dt.Rows[i],"media_id");
                                if (!string.IsNullOrEmpty(dt.Rows[i]["LastWatchedDate"].ToString()))
                                {
                                    oPersonalLastWatched.m_dLastWatchedDate = System.Convert.ToDateTime(dt.Rows[i]["LastWatchedDate"].ToString());
                                }
                                oPersonalLastWatched.m_sLastWatchedDevice = Utils.GetStrSafeVal(dt.Rows[i],"LastDeviceName");
                                oPersonalLastWatched.m_sSiteUserGuid = Utils.GetStrSafeVal(dt.Rows[i],"site_user_guid");

                                response.m_lPersonalLastWatched.Add(oPersonalLastWatched);
                            }
                        }
                    }
                }

                response.m_nTotalItems = dt.Rows.Count;
                

                xmlresult = "no resultes";
                if (response != null)
                {
                    xmlresult = SerializeToXML<PersonalLastDeviceResponse>(response);
                }
                _logger.Info(xmlresult);
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }

    }
}
