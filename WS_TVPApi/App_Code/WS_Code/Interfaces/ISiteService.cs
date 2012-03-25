using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService" here, you must also update the reference to "IService" in Web.config.
    [ServiceContract]
    public interface ISiteService
    {
        #region SiteMap
        [OperationContract]
        TVPApi.SiteMap GetSiteMap(InitializationObject initObj);
        #endregion

        [OperationContract]
        TVPApi.PageContext GetPage(InitializationObject initObj, long ID, bool withMenu, bool withFooter);

        [OperationContract]
        TVPApi.PageContext GetPageByToken(InitializationObject initObj, Pages token, bool withMenu, bool withFooter);

        [OperationContract]
        Menu GetMenu(InitializationObject initObj, long ID);

        [OperationContract]
        Menu GetFooter(InitializationObject initObj, long ID);

        [OperationContract]
        Profile GetBottomProfile(InitializationObject initObj, long ID);

        [OperationContract]
        List<TVPApi.PageGallery> GetPageGalleries(InitializationObject initObj, long PageID, int pageSize, int start_index);

        [OperationContract]
        PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID);

        [OperationContract]
        DomainResponseStatus AddUserToDomain(InitializationObject initObj, bool bMaster);

        [OperationContract]
        Domain RemoveUserFromDomain(InitializationObject initObj);

        [OperationContract]
        DomainResponseStatus AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID);

        [OperationContract]
        DomainResponseStatus RemoveDeviceFromDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID);

        [OperationContract]
        DomainResponseStatus ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive);

        [OperationContract]
        Domain GetDomainInfo(InitializationObject initObj);

        [OperationContract]
        DomainResponseStatus SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription);
    }
}
