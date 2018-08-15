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
            IngestResponse ingestResponse = null;
            log.DebugFormat("Start HandleMediaIngest. groupId:{0}", groupId);
            string notifyXml = string.Empty;

            List<MediaAsset> mediaAssets = AssetXmlSerializer.ConvertToMediaAssets(xml, groupId, out ingestResponse);
            if (mediaAssets == null || ingestResponse == null || ingestResponse.IngestStatus == null || ingestResponse.IngestStatus.Code == (int)eResponseStatus.Error)
            {
                // TODO SHIR - NO GOOD
            }
            
            // TODO SHIR - set it inside AssetXmlSerializer.ConvertToMediaAssets - what is it..
            //if (string.IsNullOrEmpty(notifyXml))
            //{
            //    log.Warn("For input " + requestData + " response is empty");
            //    return new IngestResponse() { Status = "ERROR" };
            //}
            
            try
            {
                if (ingestResponse.IngestStatus.Code == (int)eResponseStatus.OK)
                {
                    string sImporterResponse = "<importer>" + notifyXml + "</importer>";
                    if (mediaAssets.Count > 0 && ingestResponse.AssetsStatus.Count > 0)
                    {
                        ingestResponse.AssetID = mediaAssets[0].CoGuid;
                        ingestResponse.Description = ingestResponse.AssetsStatus[0].Status.Message; // message
                        ingestResponse.Status = ingestResponse.AssetsStatus[0].Status.Code.ToString(); // status
                        ingestResponse.TvmID = ingestResponse.AssetsStatus[0].ExternalAssetId; // tvm_id;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("For input " + xml + " response is " + notifyXml, ex);
                return new IngestResponse() { Status = "ERROR" };
            }
            
            return ingestResponse;
        }
    }
}
