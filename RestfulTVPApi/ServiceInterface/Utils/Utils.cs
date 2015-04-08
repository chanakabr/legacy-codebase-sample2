using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Context;
using TVPApiModule.Manager;

namespace RestfulTVPApi.ServiceInterface
{
    public static class Utils
    {
        public static bool GetUseStartDateValue(int groupId, PlatformType platform)
        {
            return bool.Parse(ConfigManager.GetInstance().GetConfig(groupId, platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate);
        }

        public static bool GetIsSingleLoginValue(int groupId, PlatformType platform)
        {
            return ConfigManager.GetInstance().GetConfig(groupId, platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return diff.TotalSeconds;
        }
    }
}