using Core.Catalog.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using GroupsCacheManager;
using Core.Catalog.Response;
using Tvinci.Core.DAL;
using System.Data;
using DAL;
using ApiObjects.Response;
using KLogMonitor;
using System.Reflection;
using Core.Users;

namespace Core.Catalog.Request
{
    [DataContract]
    public class CountryRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string Ip { get; set; }

        public CountryRequest()
            : base()
        {
        }

        public CountryRequest(string ip, int groupId, int pageSize, int pageIndex, string userIP, Filter filter, string sSignature, string sSignString)
            : base(pageSize, pageIndex, userIP, groupId, filter, sSignature, sSignString)
        {
            Ip = ip;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CountryRequest request = (CountryRequest)oBaseRequest;
            CountryResponse response = new CountryResponse();

            int countryId = 0;
            try
            {
                countryId = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(Ip);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting ip to country from ES. ip = {0}", Ip, ex);
            }


            if (countryId != 0)
            {
                DataTable dt = UsersDal.GetCountryById(countryId);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    response.Country = new Country()
                    {
                        m_nObjecrtID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]),
                        m_sCountryCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["COUNTRY_CD2"]),
                        m_sCountryName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["COUNTRY_NAME"]),
                    };
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CountryNotFound, "Country was not found");
                }
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CountryNotFound, "Country was not found");
            }
            return (BaseResponse)response;
        }
    }
}
