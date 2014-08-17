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
using TVPApiModule.Helper;
using TVPApiModule.yes.tvinci.ITProxy;
using System.Configuration;
using TVPApiModule.Objects.ORCARecommendations;
using System.Text.RegularExpressions;
using TVPPro.Configuration.OrcaRecommendations;

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
                        Domain domain = domainsService.GetDomainInfo(userResponseObject.m_user.m_domianID);
                        var deviceFamilyID = domain.m_deviceFamilies.Where(f => f.DeviceInstances != null && f.DeviceInstances.Length > 0).Select(f => f.DeviceInstances.Where(dev => dev.m_deviceName == sDeviceName).FirstOrDefault()).FirstOrDefault().m_deviceFamilyID;

                        YesObject yesObj = new YesObject()
                        {
                            AccountNumber = userResponseObject.m_user.m_oDynamicData.m_sUserData.Where(x => x.m_sDataType == "accNum").FirstOrDefault().m_sValue,
                            BrandID = 2,
                            DeviceName = sDeviceName,
                            UDID = _initObj.UDID,
                            Username = userResponseObject.m_user.m_oBasicData.m_sUserName,
                            DeviceFamilyID = deviceFamilyID
                        };

                        try
                        {
                            YesAddDeviceToDomain(yesObj);
                        }
                        catch (Exception ex)
                        {
                            DomainResponseObject statusRemove = base.RemoveDeviceToDomain();

                            resp = new DomainResponseObject() { m_oDomainResponseStatus = DomainResponseStatus.Error, m_oDomain = statusRemove.m_oDomain };

                            logger.ErrorFormat("ITProxy->AddDevice Error. Params: AccountNumber={0}, UDID={1}, Username={2}, Exception: {3}", yesObj.AccountNumber, yesObj.UDID, yesObj.Username, (ex != null && ex.InnerException != null) ? ex.InnerException.ToString() : ex.ToString());
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
                yes.tvinci.ITProxy.Entitlement[] ent = GetValidEntitlements(GetEntitlementForUserAndMedias(sUserGuid, iFileID));

                if (ent != null && ent.Length > 0) bRet = true;
            }

            return bRet;
        }

        public override string GetMediaLicenseData(int iMediaFileID, int iMediaID)
        {
            string sRet = string.Empty;
            string sError = @"{{""Error"":{0}, ""Description"":""{1}""}}";

            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(_nGroupID, _initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            dsItemInfo mediaInfo = (new APIMediaLoader(account.TVMUser, account.TVMPass, iMediaID.ToString()) { GroupID = _nGroupID, Platform = _initObj.Platform, DeviceUDID = _initObj.UDID, Language = _initObj.Locale.LocaleLanguage }.Execute());

            // Error
            if (mediaInfo.Item.Rows.Count == 0)
            {
                sRet += string.Format(sError, 1000, "MediaID '" + iMediaID + "'was not found");
                logger.ErrorFormat("ImplementationYes::GetMediaLicenseData -> {0}", sRet);

                return sRet;
            }
            else if (mediaInfo.Item[0].GetChildRows("Item_Metas").Length > 0 && !mediaInfo.Item[0].GetChildRows("Item_Metas")[0].Table.Columns.Contains("BundleID"))
            {
                sRet += string.Format(sError, 1001, "No 'BundleID' field for MediaID " + iMediaID);
                return sRet;
            }

            if (mediaInfo.Item.Rows.Count > 0 && mediaInfo.Item[0].GetChildRows("Item_Metas").Length > 0 && mediaInfo.Item[0].GetChildRows("Item_Metas")[0].Table.Columns.Contains("BundleID"))
            {
                using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
                {
                    string sBundleID = mediaInfo.Item[0].GetChildRows("Item_Metas")[0]["BundleID"].ToString();
                    string sComponentID = mediaInfo.Files.Where(r => r.FileID.Equals(iMediaFileID.ToString())).FirstOrDefault().CoGuid;

                    // Error no file component ID
                    if (string.IsNullOrEmpty(sComponentID))
                    {
                        sRet = string.Format(sError, 1002, "No ComponentID (coGUID) was found for File ID '" + iMediaFileID + "'");
                        logger.ErrorFormat("ImplementationYes::GetMediaLicenseData -> {0}", sRet);

                        return sRet;
                    }

                    yes.tvinci.ITProxy.Entitlement[] ent = GetValidEntitlements(GetEntitlementForUserAndMedias(_initObj.SiteGuid, iMediaID));

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


        public override OrcaResponse GetRecommendedMediasByGallery(InitializationObject initObj, int groupID, int mediaID, string picSize, int maxParentalLevel, eGalleryType galleryType)//TVPApiModule.Objects.Enums.eGalleryType galleryType)
        {
            logger.DebugFormat("ImplementationYes::GetRecommendedMediasByGallery -> gallery type : {0}", galleryType);

            OrcaResponse retVal = new OrcaResponse();

            // get ORCA response
            object orcaResponse = GetOrcaResponse(groupID, initObj.Platform, mediaID, maxParentalLevel, galleryType, initObj.Locale.LocaleLanguage);

            if (orcaResponse == null ||
                (orcaResponse is List<VideoRecommendation> && (orcaResponse as List<VideoRecommendation>).Count == 0) ||
                (orcaResponse is List<LiveRecommendation> && (orcaResponse as List<LiveRecommendation>).Count == 0))
            {
                logger.Error("ImplementationYes::GetRecommendedMediasByGallery -> No response from Orca");

                // get data from configuration
                var orcaConfiguration = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).OrcaRecommendationsConfiguration;
                int maxResults = orcaConfiguration.Data.MaxResults;
                int channelID;

                if (orcaResponse is List<LiveRecommendation>)
                    channelID = orcaConfiguration.Data.LiveFailOverChannelID;
                else
                    channelID = orcaConfiguration.Data.VODFailOverChannelID;

                retVal.ContentType = eContentType.VOD;
                retVal.Content = RecommendationsHelper.GetFailOverChannel(initObj, groupID, channelID, picSize, maxResults);

                return retVal;
            }

            switch (orcaResponse.GetType().ToString())
            {
                case "System.Collections.Generic.List`1[TVPApiModule.Objects.ORCARecommendations.VideoRecommendation]":
                    // VOD recommendations - return medias
                    List<VideoRecommendation> videoRecommendations = orcaResponse as List<VideoRecommendation>;
                    dsItemInfo medias = RecommendationsHelper.GetVodRecommendedMediasFromCatalog(groupID, initObj.Platform, picSize, videoRecommendations, initObj.Locale.LocaleLanguage);
                    if (medias != null && medias.Item != null)
                    {
                        Media[] orderedMedias = new Media[videoRecommendations.Count];
                        Media media;
                        int index;
                        string ibmsTitleID, ibmsSeriesID, offerID;
                        foreach (dsItemInfo.ItemRow row in medias.Item.Rows)
                        {
                            media = new Media(row, initObj, groupID, false, medias.Item.Count);
                            ibmsTitleID = GetExternalID(media.Metas.Where(m => m.Key == "IBMSTitleID").FirstOrDefault().Value);
                            ibmsSeriesID = GetExternalID(media.Metas.Where(m => m.Key == "IBMSseriesID").FirstOrDefault().Value);
                            offerID = GetExternalID(media.Tags.Where(t => t.Key == "OfferID").FirstOrDefault().Value);
                            index = videoRecommendations.FindIndex(r => r.ContentID.ToString() == ibmsTitleID || r.SeriesID == ibmsSeriesID ||
                                r.ContentID.ToString() == offerID);

                            if (index != -1)
                                orderedMedias[index] = media;
                        }

                        retVal.ContentType = eContentType.VOD;
                        retVal.Content = orderedMedias.Where(m => m != null).ToList();
                    }

                    break;
                case "System.Collections.Generic.List`1[TVPApiModule.Objects.ORCARecommendations.LiveRecommendation]":
                    // Live recommendations = return programme objects

                    retVal.ContentType = eContentType.Live;
                    retVal.Content = RecommendationsHelper.GetLiveRecommendedMedias(initObj.SiteGuid, groupID, initObj.Platform, initObj.Locale.LocaleLanguage, picSize, orcaResponse as List<LiveRecommendation>);

                    break;
                default:
                    break;
            }
            return retVal;
        }

        private string GetExternalID(string src)
        {
            if (!string.IsNullOrEmpty(src))
            {
                int start = src.LastIndexOf('/');

                if (start > 0 && src.Substring(start).Contains('-'))
                {
                    int to = src.LastIndexOf('-');
                    if (start < 0 || to < 0) return "";
                    return src.Substring(start + 1, to - start - 1);
                }
                else return src.Substring(start + 1);
            }
            else return string.Empty;
        }

        private object GetOrcaResponse(int groupID, PlatformType platform, int mediaID, int maxParentalLevel, eGalleryType galleryType, string language)
        {
            object orcaResponse = null;
            string stringOrcaResponse = null;

            // get ORCA configuration
            var orcaConfiguration = ConfigManager.GetInstance().GetConfig(groupID, platform).OrcaRecommendationsConfiguration;

            // get params for ORCA request
            string userToken = string.Empty; // Change later
            int maxResults = orcaConfiguration.Data.MaxResults;
            int defaultParentalLevel = orcaConfiguration.Data.DefaultParentalLevel;

            maxParentalLevel = maxParentalLevel == 0 ? defaultParentalLevel : maxParentalLevel;

            // get gallery configuration by gallery type
            Gallery galleryConfiguration = RecommendationsHelper.GetGalleryConfigurationByType(orcaConfiguration, galleryType);
            if (galleryConfiguration == null)
            {
                logger.ErrorFormat("No configuration for gallery type: {0}", galleryType);
                return null;
            }

            // get params from gallery configuration
            string blend = galleryConfiguration.GalleryTypeData.blend;
            string type = galleryConfiguration.GalleryTypeData.type;
            string genres = galleryConfiguration.GalleryTypeData.genres;

            TVPApiModule.yes.tvinci.ITProxy.KeyValuePair[] extraParams = RecommendationsHelper.GetExtraParamsFromConfig(galleryConfiguration.GalleryTypeData.Params.ParamCollection, mediaID, groupID, platform, language);

            try
            {
                using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
                {
                    switch (galleryConfiguration.eContentType)
                    {
                        case eContentType.VOD:
                            // get ORCA VOD response
                            stringOrcaResponse = proxy.GetVideoRecommendationList(userToken, maxResults, maxParentalLevel, blend, type, genres, extraParams);
                            // Parse ORCA xml response to video recommendations
                            orcaResponse = RecommendationsHelper.ParseOrcaResponseToVideoRecommendationList(stringOrcaResponse);
                            break;
                        case eContentType.Live:
                            // get start time and end time
                            long startTime = RecommendationsHelper.ConvertDateTimeToEpoch(DateTime.UtcNow.AddHours(orcaConfiguration.Data.GMTOffset));
                            long endTime = RecommendationsHelper.ConvertDateTimeToEpoch(RecommendationsHelper.GetEndTimeForLiveRequest(orcaConfiguration));


                            // get ORCA live response
                            stringOrcaResponse = proxy.GetLiveRecommendationList(userToken, maxResults, maxParentalLevel, blend, startTime, endTime);
                            // Parse ORCA xml response to live recommendations
                            orcaResponse = RecommendationsHelper.ParseOrcaResponseToLiveRecommendationList(stringOrcaResponse);
                            if (orcaResponse == null)
                                orcaResponse = new List<LiveRecommendation>();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("ImplementationYes::GetOrcaResponse -> {0}", ex);
            }
            return orcaResponse;

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
                string yesFamilyId = string.Empty;
                switch (yObj.DeviceFamilyID)
                {
                    case 1:
                        yesFamilyId = "3";
                        break;
                    case 2:
                    case 3:
                    case 6:
                        yesFamilyId = "4";
                        break;
                    case 4:
                        yesFamilyId = "2";
                        break;
                    case 5:
                        yesFamilyId = "1";
                        break;
                }

                proxy.AddDevice(yObj.AccountNumber, HttpUtility.UrlEncode(yObj.DeviceName), yObj.UDID, yesFamilyId, yObj.Username);
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

                        TVMAccountType account = SiteMapManager.GetInstance.GetPageData(_nGroupID, _initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
                        dsItemInfo mediaInfo = (new APIMediaLoader(account.TVMUser, account.TVMPass, iMediaID.ToString()) { GroupID = _nGroupID, Platform = _initObj.Platform, DeviceUDID = _initObj.UDID, Language = _initObj.Locale.LocaleLanguage }.Execute());

                        if (mediaInfo.Item.Count > 0 && mediaInfo.Item[0].GetChildRows("Item_Tags").Length > 0)
                        {
                            if (mediaInfo.Item[0].GetChildRows("Item_Tags")[0].Table.Columns.Contains("Product key"))
                            {
                                string[] sProductPKs = mediaInfo.Item[0].GetChildRows("Item_Tags")[0]["Product key"].ToString().Split('|');
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
            public int DeviceFamilyID { get; set; }


        }

        public override string GetMediaLicenseLink(InitializationObject initObj, int groupId, int mediaFileID, string baseLink)
        {
            string retVal = null;
            using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
            {
                retVal = proxy.GetMediaLicenseLink(initObj.SiteGuid, mediaFileID, baseLink, SiteHelper.GetClientIP(), initObj.UDID);
            }
            return retVal;
        }

        private yes.tvinci.ITProxy.Entitlement[] GetValidEntitlements(yes.tvinci.ITProxy.Entitlement[] ent)
        {
            return ent.Where(e => e.status != EntitlementStatus.DELETED && e.status != EntitlementStatus.REVOKED).ToArray();
        }

        public override RecordAllResult RecordAll(string accountNumber, string channelCode, string recordDate, string recordTime, string versionId)
        {
            RecordAllResult retVal = null;
            using (yes.tvinci.ITProxy.Service proxy = new yes.tvinci.ITProxy.Service())
            {
                retVal = proxy.RecordAll(accountNumber, channelCode, recordDate, recordTime, versionId);
            }
            return retVal;
        }
    }
}
