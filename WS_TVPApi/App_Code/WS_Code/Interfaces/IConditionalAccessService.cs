using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IConditionalAccessService
    {
        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo ActivateCampaignWithInfo(InitializationObject initObj, long campID, string hashCode, int mediaID, string mediaLink,
                                                                                                         string senderEmail, string senderName, CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents);       
    } 

}