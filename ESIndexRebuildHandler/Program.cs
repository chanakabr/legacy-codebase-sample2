using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESIndexRebuildHandler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TCMClient.Settings.Instance.Init();
            TaskHandler handler = new TaskHandler();
            string response = handler.HandleTask("{\"group_id\":147,\"switch_index_alias\":true,\"delete_old_indices\":true,\"type\":\"epg\", \"start_date\": \"20140401000000\", \"end_date\": \"20140507000000\" }");


        }
    }
}
