using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OnlyNewStandardAttribute : Attribute
    {
    }
}