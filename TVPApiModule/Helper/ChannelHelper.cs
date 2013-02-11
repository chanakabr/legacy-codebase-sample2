using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.DataEntities;

namespace TVPApiModule.Helper
{
    public class ChannelHelper
    {
        public static List<Channel> GetChannelsList(InitializationObject initObj, string picSize, int groupID)
        {
            List<Channel> lstRet = new List<Channel>();

            // get all types of channel list
            lstRet.AddRange(GetChannelsListByAccountType(picSize, groupID, initObj.Platform, AccountType.Parent));
            lstRet.AddRange(GetChannelsListByAccountType(picSize, groupID, initObj.Platform, AccountType.Regular));
            lstRet.AddRange(GetChannelsListByAccountType(picSize, groupID, initObj.Platform, AccountType.Fictivic));
            return lstRet;
        }

        private static List<Channel> GetChannelsListByAccountType(string picSize, int groupID, PlatformType platform, AccountType accountType)
        {
            List<Channel> retVal = new List<Channel>();

            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(accountType);

            // get data
            dsItemInfo _dsItemInfo = new APIChannelsListLoader(account.TVMUser, account.TVMPass, picSize).Execute();

            if (_dsItemInfo.Channel != null && _dsItemInfo.Channel.Count > 0)
            {
                foreach (dsItemInfo.ChannelRow row in _dsItemInfo.Channel)
                {
                    retVal.Add(new Channel(row));
                }
            }
            return retVal;
        }
    }
}
