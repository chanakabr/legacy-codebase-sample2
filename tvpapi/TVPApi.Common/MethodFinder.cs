using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Phx.Lib.Log;
using TVinciShared;

/// <summary>
/// Finds the Method By Reflection
/// </summary>
public partial class MethodFinder
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    /// <summary>
    /// Operator service - The service the request currently uses
    /// </summary>
    private object Webservice { get; set; }
    private object[] BackWebservice { get; set; }
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
    public MethodFinder(params object[] fromService)
    {
        BackWebservice = fromService;
        Webservice = null;
        IsPost = HttpContext.Current.Items.ContainsKey("initObj") || HttpContext.Current.Items.ContainsKey("sRecieverUDID") || HttpContext.Current.Items.ContainsKey("sUDID");
    }

    public string ProcessRequest(string sJsonFormatInput)
    {
        string result = string.Empty;
        try
        {
            string errorMessage;
            if (VerifyAllParametersCheck(out errorMessage))
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
                if (string.IsNullOrEmpty(SerializedReturnValue))
                    logger.WarnFormat("No results found or null object returned");

                result = SerializedReturnValue;
            }
            else
            {
                result = errorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.Error("Error while processing request", ex);
            result = ErrorHandler(String.Format("Exception Generated. Reason: {0}", ex.Message));
        }

        return result;
    }
}