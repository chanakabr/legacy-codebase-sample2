namespace ApiObjects.Rules.FilterActions
{
    public class FilterFileByDynamicDataInDiscovery : FilterFileByDynamicData, IFilterFileInDiscovery
    {
        public FilterFileByDynamicDataInDiscovery()
        {
            Type = RuleActionType.FilterFileByDynamicDataInDiscovery;
        }
    }
}