using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using Catalog.Cache;
using ApiObjects.MediaMarks;
using Catalog.Response;
using KLogMonitor;

namespace Catalog.Request
{
    /**************************************************************************
    * Get Personal Last Device
    * Get SiteGuid + MediaID
    * and return  : the last device name + date 
    * ************************************************************************/
    [DataContract]
    public class PersonalLasDeviceRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures doesn't match");
               
                List<UserMediaMark> lastMediaMarksList = CatalogDAL.Get_PersonalLastDevice(request.m_nMediaIDs, request.m_sSiteGuid);
                int startIndex = -1;
                int count = -1;

                if (lastMediaMarksList != null && lastMediaMarksList.Count > 0)
                {
                    bool bContinue = Utils.GetPagingValues(lastMediaMarksList.Count, request.m_nPageIndex, request.m_nPageSize, ref startIndex, ref count);
                    if (bContinue)
                    {
                        int nSiteGuid = 0;
                        int.TryParse(request.m_sSiteGuid, out nSiteGuid);
                        DataTable dtDevices = DAL.DomainDal.GetDevicesToUser(nSiteGuid);
                        if (dtDevices != null && dtDevices.Rows.Count > 0)
                        {
                            Dictionary<string, string> dictDevices = new Dictionary<string, string>(); // key: udid, value: device_name
                            foreach (DataRow rowDevice in dtDevices.Rows)
                            {
                                string sUDID = ODBCWrapper.Utils.GetSafeStr(rowDevice["device_id"]);
                                string sDeviceName = ODBCWrapper.Utils.GetSafeStr(rowDevice["Name"]);
                                if (!dictDevices.ContainsKey(sUDID))
                                {
                                    dictDevices.Add(sUDID, sDeviceName);
                                }
                            }

                            for (int i = startIndex; i < count; i++)
                            {
                                oPersonalLastWatched = new PersonalLastDevice();

                                oPersonalLastWatched.m_nID = lastMediaMarksList[i].AssetID;

                                oPersonalLastWatched.m_dLastWatchedDate = lastMediaMarksList[i].CreatedAt;


                                string sLastDeviceName = string.Empty;
                                if (dictDevices.ContainsKey(lastMediaMarksList[i].UDID))
                                {
                                    sLastDeviceName = dictDevices[lastMediaMarksList[i].UDID];
                                    if (string.IsNullOrEmpty(sLastDeviceName) || sLastDeviceName.Contains("PC||"))
                                    {
                                        sLastDeviceName = "PC";
                                    }
                                }
                                else
                                {
                                    sLastDeviceName = "No Name";
                                }

                                oPersonalLastWatched.m_sLastWatchedDevice = sLastDeviceName;

                                oPersonalLastWatched.m_sSiteUserGuid = lastMediaMarksList[i].UserID.ToString();

                                response.m_lPersonalLastWatched.Add(oPersonalLastWatched);
                            }
                        }
                    }
                }

                response.m_nTotalItems = lastMediaMarksList.Count;

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
