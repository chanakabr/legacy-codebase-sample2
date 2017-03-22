using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.ServiceModel;

namespace SeriesRecordingTaskHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";
            try
            {
                log.DebugFormat("starting SeriesRecordingTask request. data={0}", data);
                SeriesRecordingTaskRequest request = JsonConvert.DeserializeObject<SeriesRecordingTaskRequest>(data);

                switch (request.SeriesRecordingTaskType)
                {
                    case ApiObjects.eSeriesRecordingTask.FirstFollower:
                        if (Core.ConditionalAccess.Module.HandleFirstFollowerRecording(request.GroupId, request.UserId, request.DomainId, request.ChannelId, request.SeriesId, request.SeasonNumber))
                        {
                            result = "success";
                        }
                        else
                        {
                            throw new Exception(string.Format("FirstFollowerRecording request did not finish successfully, request: {0}", request.ToString()));
                        }
                        break;
                    case ApiObjects.eSeriesRecordingTask.CompleteRecordings:
                        if (Core.ConditionalAccess.Module.CompleteDomainSeriesRecordings(request.GroupId, request.DomainId))
                        {
                            result = "success";
                        }
                        else
                        {
                            throw new Exception(string.Format("CompleteDomainSeriesRecordings request did not finish successfully, request: {0}", request.ToString()));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}