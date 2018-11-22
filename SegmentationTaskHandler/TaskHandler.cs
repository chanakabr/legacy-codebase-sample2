using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;
using System.Web;
using QueueWrapper;
using ApiObjects;
using ConfigurationManager;
using ApiObjects.Segmentation;

namespace SegmentationTaskHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting segmentation task. data={0}", data);

                SegmentationTaskRequest request = JsonConvert.DeserializeObject<SegmentationTaskRequest>(data);

                bool success = false;

                switch (request.TaskType)
                {
                    case SegmentationTaskType.update_user_segments:
                        {
                            success = UserSegment.MultiInsert(request.GroupID, request.UsersSegments);
                            break;
                        }
                    default:
                        break;
                }
                if (!success)
                {
                    throw new Exception(string.Format(
                        "Segmentation task on {0} did not finish successfully.", request.TaskType));
                }
                else
                {
                    result = "success";
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
