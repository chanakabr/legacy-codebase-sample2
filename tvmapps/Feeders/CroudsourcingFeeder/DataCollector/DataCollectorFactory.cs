using System;
using System.Reflection;
using ApiObjects.CrowdsourceItems;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.DataCollector.Implementations;
using KLogMonitor;

namespace CrowdsourcingFeeder.DataCollector
{
    public static class DataCollectorFactory
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                        log.Debug("Crowdsource - " + string.Format("Collector: {0} - Error parsing channelId", collectorType));
                    break;

                case eCrowdsourceType.SlidingWindow:
                    if (int.TryParse(assetId, out channelId))
                        dataCollector = new SlidingWindowDataCollector(int.Parse(assetId), groupId);

                    else
                        log.Debug("Crowdsource - " + string.Format("Collector: {0} - Error parsing channelId", collectorType));
                    break;
                case eCrowdsourceType.Orca:

                    eGalleryType galleryType;
                    if (Enum.TryParse(assetId, true, out galleryType))
                        dataCollector = new OrcaDataCollector(groupId, galleryType);

                    else
                        log.Debug("Crowdsource - " + string.Format("Collector: {0} - Error parsing GalleryType", collectorType));
                    break;
            }
            return dataCollector;
        }
    }
}
