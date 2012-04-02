using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Web.Services.Protocols;
using System.Runtime.Serialization;
using System.Xml.Linq;

/// <summary>
/// Finds the Method By Reflection
/// </summary>
public partial class MethodFinder
{  
    #region Method Info
    private System.Web.Services.WebService Webservice { get; set; }
    private System.Web.Services.WebService BackWebservice { get; set; }
    private MethodInfo m_MetodInfo { get; set; }
    private ParameterInfo[] MethodParameters { get; set; }
    private bool IsDefaultMode { get; set; }   
    #endregion       

    private object this[String key,System.Type targetType]
    {
        get
        {                                
            return GetParam(key, targetType);       
        }
    }    

    public void Load(String methodName)
    {
        LoadMethodDetails(methodName);
    }    

    private void LoadMethodDetails(String methodName)
    {
        m_MetodInfo = Webservice.GetType().GetMethod(methodName);
        if (m_MetodInfo == null)
        {
            Webservice = BackWebservice;
            m_MetodInfo = Webservice.GetType().GetMethod(methodName);
        }
        if (m_MetodInfo != null)
        {
            MethodParameters = m_MetodInfo.GetParameters();
            Execute();
        }        
    }

    private void Execute()
    {
        object JSONMethodReturnValue = null ;
        string SerializedReturnValue = String.Empty;
        if (!IsDefaultMode)
        {
            object[] CallParameters = new object[MethodParameters.Length];
            for (int i = 0; i < MethodParameters.Length; i++)
            {
                ParameterInfo TargetParameter = MethodParameters[i];
                CallParameters[i] = this[TargetParameter.Name, TargetParameter.ParameterType];
            }
            JSONMethodReturnValue = m_MetodInfo.Invoke(Webservice, CallParameters);
            if( JSONMethodReturnValue != null )
                SerializedReturnValue = JSONSerialize(JSONMethodReturnValue);
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{Params={");
            for (int i = 0; i < MethodParameters.Length; i++)
            {
                ParameterInfo TargetParameter = MethodParameters[i];
                sb.Append(TargetParameter.Name).Append(":");
                BuildParameterObjectString(TargetParameter.ParameterType,ref sb);
            }
            sb.Append("}}");
            SerializedReturnValue = sb.ToString();
        }
        
        HttpContext.Current.Response.HeaderEncoding =
        HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
        HttpContext.Current.Response.Charset = "utf-8";
        HttpContext.Current.Response.Write(SerializedReturnValue);
    }       

    private string JSONSerialize(object SerializationTarget)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(SerializationTarget.GetType());
        using (MemoryStream ms = new MemoryStream())
        {
            serializer.WriteObject(ms, SerializationTarget);
            string Product = Encoding.UTF8.GetString(ms.ToArray());
            return Product;
        }        
    }
    
}