using Catalog.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using GroupsCacheManager;
using Catalog.Response;
using Catalog.ws_users;
using Tvinci.Core.DAL;
using System.Data;
using DAL;
using ApiObjects.Response;

namespace Catalog.Request
{
    [DataContract]
    public class CountryRequest : BaseRequest, IRequestImp
    {
        [DataMember]
        public string Ip { get; set; }

        public CountryRequest(string ip, int groupId, int pageSize, int pageIndex, string userIP, Filter filter, string sSignature, string sSignString)
            : base(pageSize, pageIndex, userIP, groupId, filter, sSignature, sSignString)
        {
            Ip = ip;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CountryRequest request = (CountryRequest)oBaseRequest;
            CountryResponse response = new CountryResponse();

            int countryId = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(Ip);

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
