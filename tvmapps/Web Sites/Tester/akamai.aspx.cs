using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class akamai : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sPath =
            "/cp94204.edgefcs.net/ondemand"; // --> valid for the cpcode
            // "videos/test"; // --> valid for the test flv
            // "videos/"; // --> valid for the videos path
            //"videos/test"; // --> valid for the test flv and blah virtual path, multiple values separated by semicolons

        Akamai.Authentication.SecureStreaming.TypeDToken token = new Akamai.Authentication.SecureStreaming.TypeDToken(
            sPath,
            new System.Net.WebClient().DownloadString("http://whatismyip.akamai.com"), // sIP
            "Token", // Profile
            "edenmaybar", // Password][
            Convert.ToInt64(0), // Time: defaults to now: time span start
            Convert.ToInt64(600), // Window (time span lenght): here set for 10 minutes
            Convert.ToInt64(0), // Duration: N/A in flash
            null);

        string url = sPath.StartsWith("/") ? // matching against a domain/application name does not require an slist
            string.Format("rtmpe://cp94204.edgefcs.net/ondemand/mp4:videos/Chantier/milyoner-slumdog-millionaire-video.mp4?auth={0}&aifp=2000", token.String) :
            string.Format("rtmpe://cp94204.edgefcs.net/ondemand/mp4:videos/Chantier/milyoner-slumdog-millionaire-video.mp4?auth={0}&aifp=2000&slist={1}", token.String, sPath);
        //url = System.Web.HttpUtility.UrlEncode(url);

    }
}
