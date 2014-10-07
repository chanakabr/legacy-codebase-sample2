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
                        dataCollector = new RealTimeViewsDataCollector(groupId);

                    else
                        Logger.Logger.Log("Crowdsource", string.Format("Collector: {0} - Error parsing channelId", collectorType), "Crowdsourcing");
                    break;

                case eCrowdsourceType.SlidingWindow:
                    if (int.TryParse(assetId, out channelId))
                        dataCollector = new SlidingWindowDataCollector(int.Parse(assetId), groupId);

                    else
                        Logger.Logger.Log("Crowdsource", string.Format("Collector: {0} - Error parsing channelId", collectorType), "Crowdsourcing");
                    break;
                case eCrowdsourceType.Orca:
                    
                    eGalleryType galleryType;
                    if (Enum.TryParse(assetId, true, out galleryType))
                        dataCollector = new OrcaDataCollector(groupId, galleryType);
                    
                    else
                        Logger.Logger.Log("Crowdsource", string.Format("Collector: {0} - Error parsing GalleryType", collectorType), "Crowdsourcing");
                    break;
            }
            return dataCollector;
        }
    }
}
