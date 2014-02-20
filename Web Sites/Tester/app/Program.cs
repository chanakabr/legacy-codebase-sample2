using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            string sPath =
                //"/cp94204.edgefcs.net/ondemand"; // --> valid for the cpcode
                // "videos/test"; // --> valid for the test flv
                // "videos/"; // --> valid for the videos path
            "videos/test;blah/"; // --> valid for the test flv and blah virtual path, multiple values separated by semicolons

            Akamai.Authentication.SecureStreaming.TypeDToken token = new Akamai.Authentication.SecureStreaming.TypeDToken(
                sPath,
                new System.Net.WebClient().DownloadString("http://whatismyip.akamai.com"), // sIP
                "Token", // Profile
                "edenmaybar", // Password
                Convert.ToInt64(0), // Time: defaults to now: time span start
                Convert.ToInt64(600), // Window (time span lenght): here set for 10 minutes
                Convert.ToInt64(0), // Duration: N/A in flash
                null);

            string url = sPath.StartsWith("/") ? // matching against a domain/application name does not require an slist
                string.Format("rtmpe://cp94204.edgefcs.net/ondemand/videos/test.flv?auth={0}&aifp=2000", token.String) :
                string.Format("rtmpe://cp94204.edgefcs.net/ondemand/videos/test.flv?auth={0}&aifp=2000&slist={1}", token.String, sPath);

            Process.Start(string.Format("http://support.akamai.com/flash/index.html?autostart=true&treatauthas=connection&faststart=true&startingbuffer=0.5&url={0}", System.Web.HttpUtility.UrlEncode(url)));

            Console.ReadLine();
        }
    }
}
