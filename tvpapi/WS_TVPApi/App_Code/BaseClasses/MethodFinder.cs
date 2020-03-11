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
using KLogMonitor;

/// <summary>
/// Finds the Method By Reflection
/// </summary>
public partial class MethodFinder
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                string SerializedReturnValue = String.Empty;

                // get the strategy to use to handle request
                ParameterInitBase executer = GetExecuter();

                object[] CallParameters = new object[MethodParameters.Length];
                for (int i = 0; i < MethodParameters.Length; i++)
                {
                    ParameterInfo targetParameter = MethodParameters[i];

                    // get the object value of the parameter
                    CallParameters[i] = executer.InitilizeParameter(targetParameter.ParameterType, targetParameter.Name);
                }

                // post handle request
                SerializedReturnValue = executer.PostParametersInit(this, MethodParameters, CallParameters);

                // log response
                if (!string.IsNullOrEmpty(SerializedReturnValue))
                    logger.DebugFormat("API Response - \n{0}", SerializedReturnValue);
                else
                    logger.DebugFormat("No results found or null object returned");

                WriteResponseBackToClient(SerializedReturnValue);
            }
        }
        catch (Exception ex)
        {
            logger.Error("Error while processing request", ex);
            ErrorHandler(String.Format("Exception Generated. Reason: {0}", ex.Message));
        }
    }

    private void WriteResponseBackToClient(string responseMsg)
    {
        HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
        HttpContext.Current.Response.Charset = "utf-8";
        HttpContext.Current.Response.Write(responseMsg);
    }
}