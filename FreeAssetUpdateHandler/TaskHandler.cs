using FreeAssetUpdateHandler.WS_CAS;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;

namespace FreeAssetUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                log.InfoFormat("starting free asset handler request. data={0}", data);

                FreeAssetUpdateRequest request = JsonConvert.DeserializeObject<FreeAssetUpdateRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                bool success = false;
                eObjectType type = eObjectType.Unknown;

                switch (request.Type)
                {
                    case ApiObjects.eObjectType.Media:
                    {
                        type = eObjectType.Media;
                        break;
                    }
                    case ApiObjects.eObjectType.Channel:
                    {
                        type = eObjectType.Channel;
                        break;
                    }
                    case ApiObjects.eObjectType.EPG:
                    {
                        type = eObjectType.EPG;
                        break;
                    }
                    case ApiObjects.eObjectType.Unknown:
                    default:
                    break;
                }

                Status status = cas.UpdateFreeFileTypesIndex(username, password,
                    type, request.AssetIds.ToArray(), request.ModuleIds.ToArray());

                if (status != null && status.Code == 0)
                {
                    success = true;
                }

                if (!success)
                {
                    throw new Exception(string.Format("Failed performing update of free asset.", string.Empty));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed performing update of free asset. data = {0} message = {1}, stack trace= {2}, target site = {3}", 
                    data, ex.Message, ex.StackTrace, ex.TargetSite), ex);
                throw ex;
            }
            
            return res;
        }
    }
}
