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
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using log4net;
using TVPApiModule.CatalogLoaders;

namespace TVPApiModule.Objects
{
    public class ImplementationYes : ImplementationBase
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ImplementationYes));

        public ImplementationYes(int nGroupID, InitializationObject initObj)
            : base(nGroupID, initObj)
        {

        }

        public override Services.ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword)
        {
            ApiUsersService.LogInResponseData response = base.SignIn(sUsername, sPassword);

            try
            {
                if (response.UserData != null && response.UserData.m_oDynamicData != null)
                {
                    string sUserType = response.UserData.m_oDynamicData.m_sUserData.Where(x => x.m_sDataType == "Type").FirstOrDefault().m_sValue;
                    using (yes.tvinci.ITProxy.Service service = new yes.tvinci.ITProxy.Service())
                    {
                        string perm = service.GetUserPermission(sUsername, sUserType);
                        new ApiUsersService(_nGroupID, _initObj.Platform).SetUserDynamicData(response.SiteGuid, "USER_PERMISSIONS", perm);
                        UserResponseObject userData = new ApiUsersService(_nGroupID, _initObj.Platform).GetUserData(response.SiteGuid);
                        if (userData != null && userData.m_user != null && userData.m_user.m_oDynamicData != null)
                        {
                            response.UserData.m_oDynamicData = userData.m_user.m_oDynamicData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return response;
        }


        public override TVPPro.SiteManager.TvinciPlatform.Domains.DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID)
        {
            DomainResponseObject resp = base.AddDeviceToDomain(sDeviceName, nDeviceBrandID);
            if (resp.m_oDomainResponseStatus == DomainResponseStatus.OK)
            {
                ApiDomainsService domainsService = new ApiDomainsService(_nGroupID, _initObj.Platform);
                ApiUsersService usersService = new ApiUsersService(_nGroupID, _initObj.Platform);

                UserResponseObject userResponseObject = usersService.GetUserData(_initObj.SiteGuid);
                if (userResponseObject.m_RespStatus == ResponseStatus.OK && resp != null && resp.m_oDomain != null)
                {
                    string sAccountNumber = resp.m_oDomain.m_sCoGuid;
                    if (!string.IsNullOrEmpty(sAccountNumber) && userResponseObject != null && userResponseObject.m_user != null && userResponseObject.m_user.m_oBasicData != null)
                    {
                        YesObject yesObj = new YesObject()
                        {
                            AccountNumber = userResponseObject.m_user.m_oDynamicData.m_sUserData.Where(x => x.m_sDataType == "accNum").FirstOrDefault().m_sValue,
                            BrandID = 2,
                            DeviceName = sDeviceName,
                            UDID = _initObj.UDID,
                            Username = userResponseObject.m_user.m_oBasicData.m_sUserName
                        };

                        try
                        {
                            YesAddDeviceToDomain(yesObj);
                        }
                        catch (Exception ex)
                        {
                            DomainResponseObject statusRemove = base.RemoveDeviceToDomain();

                            resp = new DomainResponseObject() { m_oDomainResponseStatus = DomainResponseStatus.Error, m_oDomain = statusRemove.m_oDomain };

                            logger.ErrorFormat("ITProxy->AddDevice Error. Params: AccountNumber={0}, UDID={1}, Username={2}, Exception: {3}", yesObj.AccountNumber, yesObj.UDID, yesObj.Username, (ex != null && ex.InnerException != null)? ex.InnerException.ToString() : ex.ToString()); 
                        }
                    }
                }
            }
            return resp;
        }


        public override bool IsItemPurchased(int iFileID, string sUserGuid)
        {
            bool bRet = false;

            using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
            {
                yes.tvinci.ITProxy.Entitlement[] ent = GetEntitlementForUserAndMedias(sUserGuid, iFileID);

                if (ent != null && ent.Length > 0) bRet = true;
            }

            return bRet;
        }

        public override string GetMediaLicenseData(int iMediaFileID, int iMediaID)
        {
            string sRet = string.Empty;
            string sError = @"{{""Error"":{0}, ""Description"":""{1}""}}";


            List<Media> mediaInfo = new APIMediaLoader(iMediaID, _nGroupID, _initObj.Platform, _initObj.UDID, SiteHelper.GetClientIP(), string.Empty, _initObj.Locale.LocaleLanguage)
                .Execute() as List<Media>;

            // Error
            if (mediaInfo == null || mediaInfo.Count > 0)
            {
                sRet += string.Format(sError, 1000, "MediaID '" + iMediaID + "'was not found");
                logger.ErrorFormat("ImplementationYes::GetMediaLicenseData -> {0}", sRet);

                return sRet;
            }
            else if (mediaInfo[0].Metas != null && mediaInfo[0].Metas.Count > 0 && mediaInfo[0].Metas.Where(m => m.Key == "BundleID").FirstOrDefault() == null)
            {
                sRet += string.Format(sError, 1001, "No 'BundleID' field for MediaID " + iMediaID);
                return sRet;
            }

            if (mediaInfo[0].Metas != null && mediaInfo[0].Metas.Count > 0 && mediaInfo[0].Metas.Where(m => m.Key == "BundleID").FirstOrDefault() != null)
            {
                using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
                {
                    string sBundleID = mediaInfo[0].Metas.Where(m => m.Key == "BundleID").FirstOrDefault().Value;
                    string sComponentID = mediaInfo[0].Files[0].CoGuid;
                    
                    // Error no file component ID
                    if(string.IsNullOrEmpty(sComponentID)) {
                        sRet = string.Format(sError, 1002, "No ComponentID (coGUID) was found for File ID '" + iMediaFileID + "'");
                        logger.ErrorFormat("ImplementationYes::GetMediaLicenseData -> {0}", sRet);

                        return sRet;
                    }

                    yes.tvinci.ITProxy.Entitlement[] ent = GetEntitlementForUserAndMedias(_initObj.SiteGuid, iMediaID);
                    if (ent != null && ent.Length > 0)
                    {
                        try
                        {
                            sRet = proxy.GetCustomData(_initObj.UDID, sBundleID, sComponentID, ent[0].uuid);
                            if (string.IsNullOrEmpty(sRet))
                            {
                                sRet = string.Format(sError, 2001, "No entitlement was found in Cisco VMS");
                                logger.ErrorFormat("ImplementationYes::GetMediaLicenseData -> {0}", sRet);

                                return sRet;
                            }
                        }
                        catch (Exception ex)
                        {
                            sRet = string.Format(sError, 2000, ex.ToString());
                            logger.ErrorFormat("ImplementationYes::GetMediaLicenseData -> {0}", sRet);
                        }
                    }
                }
            }

            return sRet;
        }
        //public override string MediaMark(Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action eAction, int nMediaType, int nMediaID, int nFileID, int nLocationID)
        //{
        //    string retVal = string.Empty;

        //    BillTvodResponse resp = YesBillTvod();
        //    if (resp.errorCode == string.Empty)
        //    {
        //        retVal = base.MediaMark(eAction, nMediaType, nMediaID, nFileID, nLocationID);    
        //    }
        //    return retVal;
        //}

        //public override string MediaHit(int nMediaID, int nFileID, int nLocationID)
        //{
        //    string retVal = string.Empty;

        //    BillTvodResponse resp = YesBillTvod();
        //    if (resp.errorCode == string.Empty)
        //    {
        //        retVal = base.MediaHit(nMediaID, nFileID, nLocationID);    
        //    }

        //    return retVal;
        //}


        //public override string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams)
        //{
        //    string retVal = string.Empty;

        //    using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
        //    {
        //        YesService.PurchaseSvodResponse resPurchaseSvod = yesService.PurchaseSvod(new YesService.PurchaseSvodRequest()
        //            {
        //                accountNumber = "20",
        //                deviceDNA = "123456",
        //                IBMSOfferID = "12",
        //                productID = "3dbfc577-6d43-413f-90a2-1468875ce1fd",
        //                userName = "yes@yes.co.il",
        //                deviceType = "2"
        //            });
        //        if (resPurchaseSvod.errorCode == string.Empty)
        //        {
        //            EntitleProductResponse resEntProd = YesEntitleProduct();
        //            if (resEntProd.errorCode == string.Empty)
        //            {
        //                retVal = base.ChargeUserForSubscription(dPrice, sCurrency, sSubscriptionID, sCouponCode, sIP, sExtraParams);
        //            }
        //        }
        //    }

        //    return retVal;
        //}

        //private BillTvodResponse YesBillTvod()
        //{
        //    BillTvodResponse retVal = new BillTvodResponse(); 

        //    ApiUsersService usersService = new ApiUsersService(_nGroupID, _initObj.Platform);
        //    ApiDomainsService domainsService = new ApiDomainsService(_nGroupID, _initObj.Platform);
        //    UserResponseObject userResponse = usersService.GetUserData(_initObj.SiteGuid);

        //    if (userResponse.m_RespStatus == ResponseStatus.OK)
        //    {
        //        string sAccountNumber = domainsService.GetDomainCoGuid(userResponse.m_user.m_domianID);
        //        if (!string.IsNullOrEmpty(sAccountNumber))
        //        {
        //            using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
        //            {
        //                retVal =
        //                    yesService.BillTvod(new YesService.BillTvodRequest()
        //                        {
        //                            accountNumber = sAccountNumber,
        //                            deviceDNA = _initObj.UDID,
        //                            deviceType = "!",
        //                            entitlementDate = "02.04.2012 11:58:00",
        //                            entitlementPK = "12345",
        //                            IBMSOfferID = "14",
        //                            productID = "100",
        //                            userName = "test@yes.co.il",
        //                            viewingDate = "02.04.2012 11:58:00"
        //                        });
        //            }

        //        }
        //    }

        //    return retVal;
        //}

        //private EntitleProductResponse YesEntitleProduct()
        //{
        //    using (YesService.YES_InterfaceService yesService = new YES_InterfaceService())
        //    {
        //        return yesService.EntitleProduct(new YesService.EntitleProductRequest()
        //             {
        //                 accountNumber = "20",
        //                 deviceDNA = "123456",
        //                 deviceType = "2",
        //                 IBMSOfferID = "14",
        //                 productID = "3dbfc577-6d43-413f-90a2-1468875ce1fd",
        //                 rentalHours = "36",
        //                 rentalStart = "02.04.2012 17:49:00",
        //                 userName = "yes@yes.co.il",
        //                 willExpire = YesService.YesNoType.yes
        //             });
        //    }
        //}

        private void YesAddDeviceToDomain(object obj)
        {
            YesObject yObj = obj as YesObject;
            using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
            {
                proxy.AddDevice(yObj.AccountNumber, HttpUtility.UrlEncode(yObj.DeviceName), yObj.UDID, "2", yObj.Username);
            }
        }

        private yes.tvinci.ITProxy.Entitlement[] GetEntitlementForUserAndMedias(string siteGuid, int iMediaID)
        {
            yes.tvinci.ITProxy.Entitlement[] ent = null;

            using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
            {
                ApiUsersService usersService = new ApiUsersService(_nGroupID, _initObj.Platform);
                UserResponseObject userResponseObject = usersService.GetUserData(_initObj.SiteGuid);
                if (userResponseObject != null && userResponseObject.m_user != null && userResponseObject.m_user.m_oDynamicData != null)
                {
                    UserDynamicDataContainer dynamicData = userResponseObject.m_user.m_oDynamicData.m_sUserData.Where(x => x.m_sDataType == "AccountUuid").FirstOrDefault();
                    if (dynamicData != null)
                    {
                        string sAccountUuid = dynamicData.m_sValue;

                        //TVMAccountType account = SiteMapManager.GetInstance.GetPageData(_nGroupID, _initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
                        //dsItemInfo mediaInfo = (new APIMediaLoader(account.TVMUser, account.TVMPass, iMediaID.ToString()) { GroupID = _nGroupID, Platform = _initObj.Platform, DeviceUDID = _initObj.UDID, Language = _initObj.Locale.LocaleLanguage }.Execute());

                        List<Media> mediaInfo = new APIMediaLoader(iMediaID, _nGroupID, _initObj.Platform, _initObj.UDID, SiteHelper.GetClientIP(), string.Empty, _initObj.Locale.LocaleLanguage)
                            .Execute() as List<Media>; 

                        //if (mediaInfo.Item.Count > 0 && mediaInfo.Item[0].GetChildRows("Item_Tags").Length > 0)
                        if (mediaInfo != null && mediaInfo.Count > 0 && mediaInfo[0].Tags != null && mediaInfo[0].Tags.Count > 0)
                        {
                            if (mediaInfo[0].Tags.Where(t=> t.Key == "Product key").FirstOrDefault() != null)
                            {
                                string[] sProductPKs = mediaInfo[0].Tags.Where(t => t.Key == "Product key").FirstOrDefault().Value.Split('|');
                                int[] iProductPKs = sProductPKs.Select(x => int.Parse(x)).ToArray();
                                ent = proxy.GetEntitlements(sAccountUuid, iProductPKs);
                            }
                            else
                            {
                                ent = new yes.tvinci.ITProxy.Entitlement[1];
                            }
                        }
                    }
                }
            }
            return ent;
        }
        private bool YesCheckLogin(string sUserName, string sPassword)
        {
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.116.126.212/justauth");
        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    string sPostData = string.Format("username=test1&password=YEStve1");//username={0}&password={1}", sUserName, sPassword);
        //    ASCIIEncoding encoding = new ASCIIEncoding();
        //    byte[] data = encoding.GetBytes(sPostData);
        //    request.ContentLength = data.Length;

        //    CookieContainer cookieJar = new CookieContainer();
        //    request.CookieContainer = cookieJar;

        //    using (Stream writer = request.GetRequestStream())
        //    {
        //        writer.Write(data, 0, data.Length);
        //    }

        //    String sResponse = string.Empty;
        //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        //    {
        //        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //        {
        //            sResponse = reader.ReadToEnd();
        //        }

        //        foreach (Cookie cookie in response.Cookies)
        //        {
        //            HttpCookie httpCookie = new HttpCookie(cookie.Name);
        //            httpCookie.Domain = cookie.Domain;
        //            httpCookie.Expires = cookie.Expires;
        //            httpCookie.HttpOnly = cookie.HttpOnly;
        //            httpCookie.Path = cookie.Path;
        //            httpCookie.Secure = cookie.Secure;
        //            httpCookie.Value = cookie.Value;
        //            HttpContext.Current.Response.Cookies.Add(httpCookie);
        //        }
        //    }

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
