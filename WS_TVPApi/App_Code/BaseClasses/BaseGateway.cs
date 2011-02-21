using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

/// <summary>
/// Summary description for BaseGateway
/// </summary>
public abstract class BaseGateway : System.Web.UI.Page
{
    protected abstract InitializationObject GetInitObj();

    protected abstract string GetWSUser();

    protected abstract string GetWSPass();

    protected string ParseObject(object obj, int groupID, int items, int index, long mediaCount)
    {
        string retVal = string.Empty;
        IParser parser = ParserHelper.GetParser(groupID);
        if (parser != null)
        {
            retVal = parser.Parse(obj, items, index, groupID, mediaCount);
        }
        return retVal;
    }
}
