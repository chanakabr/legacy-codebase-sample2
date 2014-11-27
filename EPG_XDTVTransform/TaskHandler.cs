using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System.Xml;
using System.IO;
using Tvinci.Core.DAL;


namespace EPG_XDTVTransform
{
    public class TaskHandler : ITaskHandler
    {

        public string HandleTask(string data)
        {
            string res = null;

            EPG_XDTVTransformRequest request = JsonConvert.DeserializeObject<EPG_XDTVTransformRequest>(data);

            // Load the xml string to XmlDocument
            string sXml = request.sXml;
            XmlDocument xmlDoc = Utils.stringToXMLDoc(sXml);

            Dictionary<int, int> channelID_DB_ALU = getALUIDs();    
            Dictionary<string, KeyValuePair<int, string>> channelDic = EpgDal.GetAllEpgChannelsDic(request.nGroupID);   
       
            XmlNodeList xmlChannel = xmlDoc.GetElementsByTagName("TVGRIDBATCH");
            string sChannelID = Utils.GetSingleNodeValue(xmlChannel[0], "GN_ID");        

            if (channelDic.ContainsKey(sChannelID))
            {
                int nChannelIDDB = channelDic[sChannelID].Key;
                if (channelID_DB_ALU.ContainsKey(nChannelIDDB))
                {
                    int nChannelIDALU = channelID_DB_ALU[nChannelIDDB];
                    string sChannelName = channelDic[sChannelID].Value;

                    //tranform to xdtv and get response to send to ALU
                    res = TransformAndResponseALU(xmlDoc, nChannelIDALU, sChannelName);
                }
                else
                {
                    Logger.Logger.Log("InsertProgramsPerChannel", string.Format("no ALU channel ID found for channel {0}. channel cannot be sent to ALU", nChannelIDDB), "EPG_XDTVTransform");
                }
            }
            return res;
        }

        private string TransformAndResponseALU(XmlDocument XMLDoc, int channelIDALU, string sChannelName)
        {
            XmlDocument xmlResult = TransformToXDTV(XMLDoc, channelIDALU, sChannelName);

            return getALUResponse(xmlResult, channelIDALU);
        }

        private XmlDocument TransformToXDTV(XmlDocument XMLDoc, int channelIDALU, string sChannelName)
        {
            string xsltLoction = Utils.GetTcmConfigValue("GraceNote_XSLT_PATH");
            XmlDocument xmlResult = new XmlDocument();
            GraceNoteTransform transformer = new GraceNoteTransform(xsltLoction);

            transformer.Init(channelIDALU, sChannelName);

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    transformer.Transform(XMLDoc, writer);
                    xmlResult.LoadXml(writer.ToString());
                }
                catch (Exception exp)
                {
                    Logger.Logger.Log("Error", string.Format("Exception in transforming xml from graceNote to ALU format", exp.Message), "EPG_XDTVTransform");
                    return null;
                }
                return xmlResult;
            }
        }

        private string getALUResponse(XmlDocument XMLDoc, int channelIDALU)
        {
            string res = "";  
            string host = Utils.GetTcmConfigValue("alcatelLucentHost");
            string sChannelID = channelIDALU.ToString();
            string compressedXml = Utils.Compress(XMLDoc.InnerXml);

            EPG_XDTVTransformResponse response = new EPG_XDTVTransformResponse(host, sChannelID, compressedXml);
            res = response.ToString();
            return res;          
        }

        private Dictionary<int, int> getALUIDs()
        {
            string sPath = Utils.GetTcmConfigValue("GraceNote_ALU_IDConvertion");
            Dictionary<int, int> channelID_DB_ALU = null;
            if (File.Exists(sPath))
            {
                string json = File.ReadAllText(@sPath);
                channelID_DB_ALU = JsonConvert.DeserializeObject<Dictionary<int, int>>(json);
            }
            else
            {
                Logger.Logger.Log("getALUIDs", string.Format("GraceNote_ALU_IDConvertion in path {0} was not found. channel cannot be sent to ALU", sPath), "EPG_XDTVTransform");
            }

            return channelID_DB_ALU;
        }

    }
}
