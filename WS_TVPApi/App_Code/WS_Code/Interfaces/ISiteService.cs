using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService" here, you must also update the reference to "IService" in Web.config.
    [ServiceContract]
    public interface ISiteService
    {
        #region SiteMap
        [OperationContract]
        TVPApi.SiteMap GetSiteMap(InitializationObject initObj, string ws_User, string ws_Pass);
        #endregion

        [OperationContract]
        TVPApi.PageContext GetPage(InitializationObject initObj, string ws_User, string ws_Pass, long ID, bool withMenu, bool withFooter);

        [OperationContract]
        TVPApi.PageContext GetPageByToken(InitializationObject initObj, string ws_User, string ws_Pass, Pages token, bool withMenu, bool withFooter);

        [OperationContract]
        Menu GetMenu(InitializationObject initObj, string ws_User, string ws_Pass, long ID);

        [OperationContract]
        Menu GetFooter(InitializationObject initObj, string ws_User, string ws_Pass, long ID);

        [OperationContract]
        Profile GetBottomProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID);

        [OperationContract]
        List<TVPApi.PageGallery> GetPageGalleries(InitializationObject initObj, string ws_User, string ws_Pass, long PageID, int pageSize, int start_index);

        [OperationContract]
        PageGallery GetGallery(InitializationObject initObj, string ws_User, string ws_Pass, long galleryID, long PageID);

        [OperationContract]
        string SignIn(InitializationObject initObj, string ws_User, string ws_Pass, string userName, string password);
    }
}
