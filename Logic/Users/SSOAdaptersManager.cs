using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Billing;
using ApiObjects.Response;
using ApiObjects.SSOAdapter;
using KLogMonitor;
using System.Reflection;

namespace APILogic.Users
{
    public static class SSOAdaptersManager
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static SSOAdaptersResponse GetSSOAdapters(int groupId)
        {
            var response = new SSOAdaptersResponse();
            try
            {
                response.SSOAdapters = DAL.UsersDal.GetSSOAdapters(groupId);
                if (response.SSOAdapters == null || !response.SSOAdapters.Any())
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK, "no sso adapters related to group");
                }
                else
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new SSOAdaptersResponse();
                response.RespStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                _Logger.Error($"Failed groupID={groupId}", ex);
            }

            return response;
        }

        public static ApiObjects.SSOAdapter.SSOAdapter InsertSSOAdapter(ApiObjects.SSOAdapter.SSOAdapter adapaterDetails, int updaterId)
        {
            try
            {
                var response = DAL.UsersDal.AddSSOAdapters(adapaterDetails, updaterId);
                return response;
            }
            catch (Exception ex)
            {
                _Logger.Error($"Failed InsertSSOAdapter groupID={adapaterDetails.GroupId}", ex);
                return null;
            }

        }
    }
}
