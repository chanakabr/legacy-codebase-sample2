using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.
    [ServiceContract]
    public interface IPlayerService
    {        
        [OperationContract]
        PlayerService.MediaWrapper GetMediaInfo(InitializationObject initObj, long MediaID, string picSize);

        [OperationContract]
        string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink);

        [OperationContract]
        string MediaMark(InitializationObject initObj, action Action, ActionHelper.FileHolder fileParam, int iLocation, long programId);

        [OperationContract]
        string MediaHit(InitializationObject initObj, long iMediaID, long iFileID, int iLocation, long programId);

        [OperationContract]
        string MediaLastPosition(InitializationObject initObj, int MediaId);

        [OperationContract]
        void MediaError(InitializationObject initObj, TVPApi.ActionHelper.FileHolder fileParam, string errorCode, string errorMessage, int location);

        [OperationContract]
        void Log(InitializationObject initObj, TVPApiServices.PlayerService.ErrorMessageWrapper message);

        [OperationContract]
        bool CheckParentalPIN(InitializationObject initObj, int ruleID, string parentalPIN);
    }
}
