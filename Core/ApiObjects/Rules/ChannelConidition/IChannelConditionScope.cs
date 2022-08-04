using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public interface IChannelConditionScope : IConditionScope
    {
        long MediaId { get; set; }
        int GroupId { get; set; }
        List<long> GetChannelsByMediald(int groupId, long mediaId);        
    }
}