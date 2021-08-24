using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Pricing.Dto
{
    // PreviewModule is on apiLogice so we cant use it on dal
    public class PreviewModuleDTO
    {
        public long Id { get; }
        public string Name { get; }
        public int FullLifeCycle { get; }
        public int NonRenewPeriod { get; }

        public PreviewModuleDTO(string name, int fullLifeCycle, int nonRenewPeriod, long id = 0)
        {
            Id = id;
            Name = name;
            FullLifeCycle = fullLifeCycle;
            NonRenewPeriod = nonRenewPeriod;
        }
    }
}
