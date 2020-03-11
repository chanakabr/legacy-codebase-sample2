using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
public class RequestDescription : System.Attribute
{
    public string paramName, paramDesc;
     
    public RequestDescription(string paramName, string paramDesc)
    {
        this.paramName = paramName;
        this.paramDesc = paramDesc;
    }
}