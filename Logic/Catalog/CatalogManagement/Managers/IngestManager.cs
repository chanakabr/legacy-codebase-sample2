using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using QueueWrapper;

namespace Core.Catalog.CatalogManagement
{
    public class IngestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static IngestResponse HandleMediaIngest(int groupId, string xml)
        {
            log.DebugFormat("Start HandleMediaIngest. groupId:{0}", groupId);
            string notifyXml = string.Empty;

            IngestResponse ingestResponse = AssetXmlSerializer.ConvertToMediaAssets(xml, groupId);
            
            if (ingestResponse == null || string.IsNullOrEmpty(ingestResponse.Description))
            {
                log.Warn("For input " + xml + " response is empty");
                return new IngestResponse() { Status = "ERROR" };
            }

            if (ingestResponse.IngestStatus == null || ingestResponse.IngestStatus.Code == (int)eResponseStatus.Error)
            {
                // TODO SHIR - SET SOME ERROR
            }

            log.DebugFormat("End HandleMediaIngest. groupId:{0}", groupId);

            return ingestResponse;
        }
    }
}
