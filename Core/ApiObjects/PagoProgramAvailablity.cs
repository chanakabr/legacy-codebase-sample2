using System;
using System.Collections.Generic;
using ProtoBuf;

[module: CompatibilityLevel(CompatibilityLevel.Level300)]
namespace ApiObjects
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic,SkipConstructor = true)]
    public class PagoProgramAvailability
    {
        public List<long> FileIds;
        public long PagoId;
        public DateTime StartDate;
        public DateTime EndDate;

        public bool IsValid()
        {
            return IsStartDateSet() && IsEndDateSet() && FileIds.Count > 0;
        }

        public bool IsStartDateSet()
        {
            return IsDateSet(StartDate);
        }
        
        public bool IsEndDateSet()
        {
            return IsDateSet(EndDate);
        }
        
        private static bool IsDateSet(DateTime d)
        {
            return d != default;
        }
    }
}