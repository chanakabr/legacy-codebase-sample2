using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TVPPro.SiteManager.Context
{
    public class EnumAsStringValue: System.Attribute 
    { 
        private string _value; 
        public EnumAsStringValue(string value) 
        { 
            _value = value; 
        } 
        public string Value 
        { 
            get 
            { 
                return _value; 
            } 
        }
    }

    public static class StringEnum
    {
        public static string GetStringValue(Enum value)
        {
            string outputString = null; 
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            EnumAsStringValue[] attrs = fi.GetCustomAttributes(typeof(EnumAsStringValue), false) as EnumAsStringValue[]; 
            if (attrs.Length > 0) 
            {
                outputString = attrs[0].Value; 
            }
            return outputString;
        }

    }
}
