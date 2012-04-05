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
    /// <summary>
    /// Operter service - The service the reuqest currently uses
    /// </summary>
    private System.Web.Services.WebService Webservice { get; set; }
    private System.Web.Services.WebService[] BackWebservice { get; set; }
    private MethodInfo m_MetodInfo { get; set; }
    private ParameterInfo[] MethodParameters { get; set; }
    /// <summary>
    /// Indicates the request type from client
    /// </summary>
    private bool IsPost { get; set; }   
    #endregion       
    /// <summary>
    /// Gets a set of web services to retrieve functions from
    /// </summary>
    /// <param name="fromService"></param>
    public MethodFinder(params System.Web.Services.WebService[] fromService)
    {
        BackWebservice = fromService;
        Webservice = null;
        IsPost = HttpContext.Current.Request.RequestType.ToLower() == "post";        
    }           

    public void ProcessRequest()
    {
        try
        {
            if (VerifyAllParametersCheck())
            {
                string SerializedReturnValue = String.Empty;
                ParameterInitBase executer = GetExecuter();//get the stratagy to use to handle reuqest

                object[] CallParameters = new object[MethodParameters.Length];
                for (int i = 0; i < MethodParameters.Length; i++)
                {
                    ParameterInfo TargetParameter = MethodParameters[i];
                    CallParameters[i] = executer.InitilizeParameter(TargetParameter.ParameterType, TargetParameter.Name);//get the object value of the parameter
                }
                SerializedReturnValue = executer.PostParametersInit(this, MethodParameters, CallParameters);//post handle request

                WriteResponseBackToClient(SerializedReturnValue);
            }            
        }
        catch (Exception e)
        {
            ErrorHandler(String.Format("Exception Genrated. Reason: {0}",e.Message));
        }
    }

    private void WriteResponseBackToClient(string responseMsg)
    {
        HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
        HttpContext.Current.Response.Charset = "utf-8";
        HttpContext.Current.Response.Write(responseMsg);
    }
    
    
}