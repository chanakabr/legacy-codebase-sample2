using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public interface IFileTypeConditionScope : IConditionScope
    {
        long MediaId { get; set; }
        int GroupId { get; set; }
        List<long> FileTypeIds { get; set; }
    }
}