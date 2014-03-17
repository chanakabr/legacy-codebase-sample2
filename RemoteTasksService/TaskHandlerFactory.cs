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
                case "resize_image":

                    return new ImageResizeHandler.TaskHandler();

                case "upload_file":

                    return new FileUploadHandler.TaskHandler();
            }

            return null;
        }
    }
    
}