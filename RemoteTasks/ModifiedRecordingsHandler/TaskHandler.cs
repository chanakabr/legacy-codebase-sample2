using ApiObjects.TimeShiftedTv;
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

namespace ModifiedRecordingsHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";
            try
            {
                log.DebugFormat("starting ModifiedRecording request. data={0}", data);
                ModifiedRecordingRequest request = JsonConvert.DeserializeObject<ModifiedRecordingRequest>(data);
                HandleDomainQuataByRecordingTask expiredRecording = new HandleDomainQuataByRecordingTask()
                {
                    Id = request.Id,
                    RecordingId = request.RecordingId,
                    GroupId = request.GroupId,
                    ScheduledExpirationEpoch = request.ScheduledExpirationEpoch,
                    OldRecordingDuration = request.OldRecordingDuration
                };

                if (Core.ConditionalAccess.Module.HandleDomainQuotaByRecording(expiredRecording))
                {
                    result = "success";
                }
                else
                {
                    throw new Exception(string.Format("ModifiedRecording request did not finish successfully, request: {0}", request.ToString()));
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