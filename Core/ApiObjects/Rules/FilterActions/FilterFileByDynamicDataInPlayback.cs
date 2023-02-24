namespace ApiObjects.Rules.FilterActions
{
    public class FilterFileByDynamicDataInPlayback : FilterFileByDynamicData, IFilterFileInPlayback
    {
        public FilterFileByDynamicDataInPlayback()
        {
            Type = RuleActionType.FilterFileByDynamicDataInPlayback;
        }
    }
}