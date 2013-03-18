using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPApiModule.YesService;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiModule.Objects
{
    public class ImplementationYes : ImplementationBase
    {
        public ImplementationYes(int nGroupID, InitializationObject initObj)
            : base(nGroupID, initObj)
        {

        }

        public override Services.ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword)
        {
            if (YesCheckLogin(sUsername, sPassword))
            {
                return base.SignIn(sUsername, sPassword);
            }
            return new ApiUsersService.LogInResponseData();
        }


        public override TVPPro.SiteManager.TvinciPlatform.Domains.DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID)
        {
            DomainResponseObject resp = base.AddDeviceToDomain(sDeviceName, nDeviceBrandID);
            if (resp.m_oDomainResponseStatus == DomainResponseStatus.OK)
            {
                ApiDomainsService domainsService = new ApiDomainsService(_nGroupID, _initObj.Platform);
                ApiUsersService usersService = new ApiUsersService(_nGroupID, _initObj.Platform);

                UserResponseObject userResponseObject = usersService.GetUserData(_initObj.SiteGuid);
                if (userResponseObject.m_RespStatus == ResponseStatus.OK)
                {
                    string sAccountNumber = domainsService.GetDomainCoGuid(userResponseObject.m_user.m_domianID);
                    if (!string.IsNullOrEmpty(sAccountNumber))
                    {
                        Thread t = new Thread(new ParameterizedThreadStart(YesAddDeviceToDomain));
                        t.Start();
                    }
                }
            }
            return resp;
        }

        public override string MediaMark(Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action eAction, int nMediaType, int nMediaID, int nFileID, int nLocationID)
        {
            string retVal = string.Empty;

            BillTvodResponse resp = YesBillTvod();
            if (resp.errorCode == string.Empty)
            {
                retVal = base.MediaMark(eAction, nMediaType, nMediaID, nFileID, nLocationID);    
            }
            return retVal;
        }

        public override string MediaHit(int nMediaID, int nFileID, int nLocationID)
        {
            string retVal = string.Empty;

            BillTvodResponse resp = YesBillTvod();
            if (resp.errorCode == string.Empty)
            {
                retVal = base.MediaHit(nMediaID, nFileID, nLocationID);    
            }

            return retVal;
        }


        public override string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams)
        {
            string retVal = string.Empty;

            using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
            {
                YesService.PurchaseSvodResponse resPurchaseSvod = yesService.PurchaseSvod(new YesService.PurchaseSvodRequest()
                    {
                        accountNumber = "20",
                        deviceDNA = "123456",
                        IBMSOfferID = "12",
                        productID = "3dbfc577-6d43-413f-90a2-1468875ce1fd",
                        userName = "yes@yes.co.il",
                        deviceType = "2"
                    });
                if (resPurchaseSvod.errorCode == string.Empty)
                {
                    EntitleProductResponse resEntProd = YesEntitleProduct();
                    if (resEntProd.errorCode == string.Empty)
                    {
                        retVal = base.ChargeUserForSubscription(dPrice, sCurrency, sSubscriptionID, sCouponCode, sIP, sExtraParams);
                    }
                }
            }

            return retVal;
        }

        private BillTvodResponse YesBillTvod()
        {
            BillTvodResponse retVal = new BillTvodResponse(); 

            ApiUsersService usersService = new ApiUsersService(_nGroupID, _initObj.Platform);
            ApiDomainsService domainsService = new ApiDomainsService(_nGroupID, _initObj.Platform);
            UserResponseObject userResponse = usersService.GetUserData(_initObj.SiteGuid);

            if (userResponse.m_RespStatus == ResponseStatus.OK)
            {
                string sAccountNumber = domainsService.GetDomainCoGuid(userResponse.m_user.m_domianID);
                if (!string.IsNullOrEmpty(sAccountNumber))
                {
                    using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
                    {
                        retVal =
                            yesService.BillTvod(new YesService.BillTvodRequest()
                                {
                                    accountNumber = sAccountNumber,
                                    deviceDNA = _initObj.UDID,
                                    deviceType = "!",
                                    entitlementDate = "02.04.2012 11:58:00",
                                    entitlementPK = "12345",
                                    IBMSOfferID = "14",
                                    productID = "100",
                                    userName = "test@yes.co.il",
                                    viewingDate = "02.04.2012 11:58:00"
                                });
                    }

                }
            }

            return retVal;
        }

        private EntitleProductResponse YesEntitleProduct()
        {
            using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
            {
                return yesService.EntitleProduct(new YesService.EntitleProductRequest()
                     {
                         accountNumber = "20",
                         deviceDNA = "123456",
                         deviceType = "2",
                         IBMSOfferID = "14",
                         productID = "3dbfc577-6d43-413f-90a2-1468875ce1fd",
                         rentalHours = "36",
                         rentalStart = "02.04.2012 17:49:00",
                         userName = "yes@yes.co.il",
                         willExpire = YesService.YesNoType.yes
                     });
            }
        }

        private void YesAddDeviceToDomain(object obj)
        {
            YesObject yObj = obj as YesObject;
            using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
            {
                YesService.AddDeviceResponse resAddDeviceResponse = yesService.AddDevice(new YesService.AddDeviceRequest()
                {
                    userName = yObj.Username,
                    accountNumber = yObj.AccountNumber,
                    device = new YesService.AddDeviceRequestDevice
                    {
                        deviceName = yObj.DeviceName,
                        drmImplementation = "PlayReady"/*Always*/,
                        deviceDNA = yObj.UDID,
                        deviceType = yObj.BrandID.ToString(),
                        isActive = YesService.YesNoType.yes
                    }
                });
            }
        }

        private bool YesCheckLogin(string sUserName, string sPassword)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.116.126.212/justauth");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string sPostData = string.Format("username=test1&password=YEStve1");//username={0}&password={1}", sUserName, sPassword);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(sPostData);
            request.ContentLength = data.Length;

            CookieContainer cookieJar = new CookieContainer();
            request.CookieContainer = cookieJar;

            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(data, 0, data.Length);
            }

            String sResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    sResponse = reader.ReadToEnd();
                }

                foreach (Cookie cookie in response.Cookies)
                {
                    HttpCookie httpCookie = new HttpCookie(cookie.Name);
                    httpCookie.Domain = cookie.Domain;
                    httpCookie.Expires = cookie.Expires;
                    httpCookie.HttpOnly = cookie.HttpOnly;
                    httpCookie.Path = cookie.Path;
                    httpCookie.Secure = cookie.Secure;
                    httpCookie.Value = cookie.Value;
                    HttpContext.Current.Response.Cookies.Add(httpCookie);
                }
            }

            return true;

        }



        private class YesObject
        {

            public string Username { get; set; }
            public string AccountNumber { get; set; }
            public string UDID { get; set; }
            public string DeviceName { get; set; }
            public int BrandID { get; set; }

        }

    }
}
