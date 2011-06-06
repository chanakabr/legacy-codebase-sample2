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
        string SignIn(InitializationObject initObj, string userName, string password);
    }
}
