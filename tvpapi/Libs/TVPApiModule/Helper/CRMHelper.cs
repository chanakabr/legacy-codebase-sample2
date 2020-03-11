using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TVPApiModule.Objects.CRM;
using TVPApiModule.Services;

namespace TVPApi
{
    public class CRMHelper
    {
        public static Core.Users.UserBasicData[] SearchUsers(int groupId, string text)
        {
            string[] sTerms = null;
            string[] sFields = null;

            bool bIsExact = false;

            string sText = text.Trim();

            //check if the serach is on a specific field
            string[] sTexts = sText.Split(new string[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (sTexts.Length  == 2)
            {
                sFields = new string[] { sTexts[0].Trim() };

                sText = sTexts[1].Trim();
            }

            //check if exact search
            MatchCollection matches = Regex.Matches(sText, @"(?<=([\'\""])).*?(?=\1)");

            if (matches.Count > 0)
            {
                bIsExact = true;

                sTerms = new string[] { matches[0].Value };
            }
            else //check if OR search
            {
                sTerms = sText.ToLower().Split(new string[] { " or " }, StringSplitOptions.RemoveEmptyEntries).Select( x => x.Trim()).ToArray();
            }

            return new ApiUsersService(groupId, PlatformType.Web).SearchUsers(sTerms, sFields, bIsExact);
        }
    }
}
