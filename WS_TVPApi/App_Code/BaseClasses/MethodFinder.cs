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
using Logger;

/// <summary>
/// Finds the Method By Reflection
/// </summary>
public partial class MethodFinder
{
    #region Method Info
    /// <summary>
    /// Operator service - The service the request currently uses
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
        IsPost = HttpContext.Current.Items.Contains("initObj") || HttpContext.Current.Items.Contains("sRecieverUDID") || HttpContext.Current.Items.Contains("sUDID");
    }

    public void ProcessRequest(string sJsonFormatInput)
    {
        try
        {
            if (VerifyAllParametersCheck())
            {
                using (BaseLog log = new BaseLog(eLogType.SoapRequest, DateTime.UtcNow, false))
                {
                    log.Id = HttpContext.Current.ApplicationInstance.Session.SessionID;
                    log.IP = HttpContext.Current.Request.UserHostAddress;
                    log.UserAgent = HttpContext.Current.Request.UserAgent;
                    log.Method = HttpContext.Current.Request.QueryString["m"];
                    log.Info(sJsonFormatInput.Replace('\n', ' '), false);

                    string SerializedReturnValue = String.Empty;

                    // get the strategy to use to handle request
                    ParameterInitBase executer = GetExecuter();

                    object[] CallParameters = new object[MethodParameters.Length];
                    for (int i = 0; i < MethodParameters.Length; i++)
                    {
                        ParameterInfo TargetParameter = MethodParameters[i];

                        // get the object value of the parameter
                        CallParameters[i] = executer.InitilizeParameter(TargetParameter.ParameterType, TargetParameter.Name);
                    }

                    // post handle request
                    SerializedReturnValue = executer.PostParametersInit(this, MethodParameters, CallParameters);
                    log.Type = eLogType.SoapResponse;
                    if (!string.IsNullOrEmpty(SerializedReturnValue))
                    {
                        log.Info(SerializedReturnValue, false);
                    }
                    else
                    {
                        log.Info("No results found or null object returned", false);
                    }                    

                    WriteResponseBackToClient(SerializedReturnValue);
                }
            }
        }
        catch (Exception e)
        {
            ErrorHandler(String.Format("Exception Generated. Reason: {0}", e.Message));
        }
    }

    private void WriteResponseBackToClient(string responseMsg)
    {
        HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
        HttpContext.Current.Response.Charset = "utf-8";
        HttpContext.Current.Response.Write(responseMsg);
    }
}