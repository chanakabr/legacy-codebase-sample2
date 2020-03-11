using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("eventNotificationAction")]
    public class EventNotificationActionController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Dispatches event notification
        /// </summary>
        /// <returns></returns>
        /// <param name="scope">Scope</param>
        [Action("dispatch")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool Dispatch(KalturaEventNotificationScope scope)
        {
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;

            try
            {
                return ClientsManager.NotificationClient().DispatchEventNotification(groupId, scope);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return false;
        }
    }
}
