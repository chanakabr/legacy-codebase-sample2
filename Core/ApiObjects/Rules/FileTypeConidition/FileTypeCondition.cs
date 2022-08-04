using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    [Serializable]
    public class FileTypeCondition : RuleCondition
    {
        public List<long> FileTypeIds { get; set; }

        public FileTypeCondition()
        {
            Type = RuleConditionType.FileType;
        }
    }
}