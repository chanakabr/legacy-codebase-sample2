using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BlockHttpMethodsAttribute : Attribute
    {
        public List<string> HttpMethods { get; set; }

        public BlockHttpMethodsAttribute(string httpMethod)
            : base()
        {
            HttpMethods = new List<string>() { httpMethod.ToLower() };
        }
        public BlockHttpMethodsAttribute(List<string> httpMethods)
            : base()
        {
            if (httpMethods != null)
                HttpMethods = httpMethods.Select(hm => hm.ToLower()).ToList();
        }
    }
}