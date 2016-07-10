using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.Filters;

namespace WebAPI.Managers.Schema
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OnlyNewStandardAttribute : Attribute
    {
    }
}