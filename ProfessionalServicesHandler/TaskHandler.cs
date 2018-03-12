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

namespace ProfessionalServicesHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                bool success = false;

                log.DebugFormat("Starting professional services request. data={0}", data);

                ProfessionalServicesRequest request = JsonConvert.DeserializeObject<ProfessionalServicesRequest>(data);
                ProfessionalServicesActionConfiguration setting = null;

                try
                {
                    object actionConfigurationJson = TCMClient.Settings.Instance.GetValue<object>(string.Format("professional_services_tasks.{0}", request.ActionImplementation));
                    setting = Newtonsoft.Json.JsonConvert.DeserializeObject<ProfessionalServicesActionConfiguration>(actionConfigurationJson.ToString());
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed reading TCM value of PS action implementation: {0}", ex);
                }

                ITaskHandler newTaskHandler = null;


                if (setting != null)
                {
                    try
                    {
                        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

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

                        Type handlerType = actionAssembly.GetType(setting.Type);

                        newTaskHandler = (ITaskHandler)Activator.CreateInstance(handlerType);

                        
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed loading specific action implementation from assembly. location = {0}, type = {1}, ex = {2}",
                            setting.DllLocation, setting.Type, ex);
                        success = false;
                    }

                    try
                    {
                        if (newTaskHandler != null)
                        {
                            result = newTaskHandler.HandleTask(data);
                            success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed handling professional services request for action {0}. ex = {1}",
                            request.ActionImplementation, ex);
                        success = false;
                    }
                }

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Professional Services request on {0} did not finish successfully.", request.ActionImplementation));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        #endregion
    }
}