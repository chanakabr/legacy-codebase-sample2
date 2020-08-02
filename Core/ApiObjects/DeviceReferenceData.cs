using System;
using ApiObjects.Base;

namespace ApiObjects
{
    public class DeviceReferenceData : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool? Status { get; set; }
        public virtual int GetType() { return -1; }

        public bool CompareAndFill(DeviceReferenceData newObject)
        {
            if (newObject == null)
            {
                return false;
            }

            Name = newObject.Name ?? Name;
            Status = newObject.Status ?? Status;

            return true;
        }
    }

    public class DeviceModelInformation : DeviceReferenceData
    {
        public override int GetType()
        {
            return (int)DeviceInformationType.Model;
        }
    }

    public class DeviceManufacturerInformation : DeviceReferenceData
    {
        public override int GetType()
        {
            return (int)DeviceInformationType.Manufacturer;
        }
    }

    public enum DeviceInformationType
    {
        Model, Manufacturer
    }
}
