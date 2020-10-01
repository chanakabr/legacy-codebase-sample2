using System;
using ApiObjects.Base;

namespace ApiObjects
{
    public class DeviceReferenceData : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool? Status { get; set; }
        public int Type { get; set; } 

        public virtual int GetReferenceType()
        {
            return -1;
        }
    }

    public class DeviceManufacturerInformation : DeviceReferenceData {
        public override int GetReferenceType()
        {
            return (int)DeviceInformationType.Manufacturer;
        }
    }

    public enum DeviceInformationType
    {
        Manufacturer = 1
    }
}
