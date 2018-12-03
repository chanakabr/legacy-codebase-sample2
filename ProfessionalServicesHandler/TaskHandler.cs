using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using System.Net;
using System.Web;
using System.ServiceModel;
using ApiObjects.Response;
using System.IO;
using System.Net.Http;
using ConfigurationManager;
using Newtonsoft.Json.Linq;

namespace ProfessionalServicesHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HttpClient client = new HttpClient();

        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            var result = "failure";
            log.DebugFormat("Starting professional services request. data={0}", data);

            var request = JsonConvert.DeserializeObject<ProfessionalServicesRequest>(data);

            var setting = GetSettings(request);
            if (setting == null)
            {
                log.ErrorFormat("could not retrieve settings for PS Handler");
                throw new Exception(string.Format("Professional Services request on {0} did not finish successfully. (could not retrieve settings for PS Handler)", request.ActionImplementation));
            }

            if (!string.IsNullOrEmpty(setting.HandlerUrl))
            {
                log.InfoFormat("Sending POST to url:[{0}], with data:[{1}]", setting.HandlerUrl, data);
                result = HttpPost(setting.HandlerUrl, data, "application/json");
            }
            else
            {
                log.InfoFormat("Trying to load dll:[{0}], type:[{1}]", setting.DllLocation, setting.Type);
                var newTaskHandler = GetTaskHandler(setting);
                log.InfoFormat("Executing external PS handler");
                result = TryExecuteExternalTaskHandler(data, newTaskHandler, result, request);
            }

            log.InfoFormat("Got response:[{}]", result);
            return result;
        }

        #endregion

        #region private methods
        private static string TryExecuteExternalTaskHandler(string data, ITaskHandler newTaskHandler, string result, ProfessionalServicesRequest request)
        {
            try
            {
                if (newTaskHandler != null)
                {
                    result = newTaskHandler.HandleTask(data);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed handling professional services request for action {0}. ex = {1}",
                    request.ActionImplementation, ex);
                throw new Exception(string.Format(
                    "Professional Services request on {0} did not finish successfully.", request.ActionImplementation));
            }

            return result;
        }

        private static ProfessionalServicesActionConfiguration GetSettings(ProfessionalServicesRequest request)
        {
            try
            {
                var actionConfigurationJson = ApplicationConfiguration.ProfessionalServicesTasksConfiguration.GetActionHandler(request.ActionImplementation);
                return actionConfigurationJson.ToObject<ProfessionalServicesActionConfiguration>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed reading TCM value of PS action implementation: {0}", ex);
                return null;
            }

        }

        private static ITaskHandler GetTaskHandler(ProfessionalServicesActionConfiguration setting)
        {
            try
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                Assembly actionAssembly = null;

                // if we have a dll location defined
                if (!string.IsNullOrEmpty(setting.DllLocation))
                {
                    // First treat it as a full path
                    if (File.Exists(setting.DllLocation))
                    {
                        actionAssembly = Assembly.LoadFrom(setting.DllLocation);
                    }
                    else
                    {
                        // Otherwise treat is as a relative path, and combine it with the base directory of the application
                        string combinedPath = string.Format("{0}{1}", baseDirectory, setting.DllLocation);

                        if (File.Exists(combinedPath))
                        {
                            actionAssembly = Assembly.LoadFrom(combinedPath);
                        }
                    }
                }

                // If we don't have a valid location, use calling assembly
                if (actionAssembly == null)
                {
                    actionAssembly = Assembly.GetCallingAssembly();
                }

                var handlerType = actionAssembly.GetType(setting.Type);

                var newTaskHandler = (ITaskHandler)Activator.CreateInstance(handlerType);
                return newTaskHandler;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed loading specific action implementation from assembly. location = {0}, type = {1}, ex = {2}",
                    setting.DllLocation, setting.Type, ex);
                return null;
            }
        }

        private string HttpPost(string uri, string parameters, string contentType = null)
        {
            try
            {
                var httpContent = new StringContent(parameters, Encoding.UTF8, contentType);
                var response = client.PostAsync(uri, httpContent).GetAwaiter().GetResult();
                var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return responseString;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on post request. URL: {0}, Parameter: {1}. Error: {2}", uri, parameters, ex);
            }
            return null;
        }
        #endregion

    }
}