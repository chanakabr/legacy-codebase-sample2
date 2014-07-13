using System;
using ApiObjects.CrowdsourceItems;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.DataCollector.Implementations;

namespace CrowdsourcingFeeder.DataCollector
{
    public static class DataCollectorFactory
    {
        public static BaseDataCollector GetInstance(eCrowdsourceType collectorType, string assetId, int groupId)
        {
            int channelId;
            BaseDataCollector dataCollector = null;
            switch (collectorType)
            {
                case eCrowdsourceType.LiveViews:
                    if (int.TryParse(assetId, out channelId))
                        dataCollector = new RealTimeViewsDataCollector(int.Parse(assetId), groupId);

                    else
                        Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error parsing channelId", DateTime.UtcNow, collectorType), "Crowdsourcing");
                    break;

                case eCrowdsourceType.SlidingWindow:
                    if (int.TryParse(assetId, out channelId))
                        dataCollector = new SlidingWindowDataCollector(int.Parse(assetId), groupId);

                    else
                        Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error parsing channelId", DateTime.UtcNow, collectorType), "Crowdsourcing");
                    break;
                case eCrowdsourceType.Orca:
                    
                    eGalleryType galleryType;
                    if (Enum.TryParse(assetId, true, out galleryType))
                        dataCollector = new OrcaDataCollector(groupId, galleryType);
                    
                    else
                        Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error parsing GalleryType", DateTime.UtcNow, collectorType), "Crowdsourcing");
                    break;
            }
            return dataCollector;
        }
    }
}
