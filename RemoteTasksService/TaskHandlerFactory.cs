using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RemoteTasksCommon;

namespace RemoteTasksService
{
    public class TaskHandlerFactory
    {
        public static ITaskHandler GetHandler(string taskType)
        {
            switch (taskType.ToLower())
            {
                case "tasks.resize_image":

                    return new ImageResizeHandler.TaskHandler();

                case "tasks.upload_image":

                    return new FileUploadHandler.TaskHandler();
            }

            return null;
        }
    }
    
}