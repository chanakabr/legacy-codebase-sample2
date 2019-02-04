using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public ExcelPropertyAttribute() : base()
        {
            Name = string.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelObjectPropertyAttribute : ExcelPropertyAttribute
    {
        public ExcelObjectPropertyAttribute() : base()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelKeyPropertyAttribute : ExcelPropertyAttribute
    {
        public ExcelKeyPropertyAttribute() : base()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelValuePropertyAttribute : ExcelPropertyAttribute
    {
        public ExcelValuePropertyAttribute() : base()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelArrayAttribute : ExcelPropertyAttribute
    {
        public ExcelArrayAttribute() : base()
        {
        }
    }
}