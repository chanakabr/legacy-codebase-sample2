using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TVPApiModule.Objects
{
    public class Enums
    {
        public enum eCutWith
        {
            OR = 0,
            AND = 1
        }

        public enum ProgramIdType
        {
            EXTERNAL = 0,
            INTERNAL = 1
        }

        public enum eCode
        {
            Success = 0,
            Failure = 1,
            BadArguments = 2
        }

        [Serializable]
        public enum eAssetFilterTypes
        {
            [EnumMember]
            EPG = 0,

            [EnumMember]
            NPVR = 1
        }
    }
}
