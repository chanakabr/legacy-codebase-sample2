using System;

namespace WebAPI.Managers.Scheme
{
    // if ListResponse does not set this Attribute so ObjectsDescription will be as base ListResponse
    [AttributeUsage(AttributeTargets.Class)]
    public class ListResponseAttribute : Attribute
    {
        public string ObjectsDescription { get; set; }

        public ListResponseAttribute(string objectsDescription)
        {
            ObjectsDescription = objectsDescription;
        }
    }
}