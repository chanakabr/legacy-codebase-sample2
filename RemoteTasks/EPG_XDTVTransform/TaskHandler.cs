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
using System.IO.Compression;
using ApiObjects.Epg;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace EPG_XDTVTransform
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = null;

            EPG_XDTVTransformRequest request = JsonConvert.DeserializeObject<EPG_XDTVTransformRequest>(data);

            string sXml = Utils.Decompress(request.sXml);

            log.Debug("Info - " + string.Concat("Received EPG XDTV: ", sXml));

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(sXml);

            Dictionary<int, int> channelID_DB_ALU = getALUIDs();
            Dictionary<string, List<EpgChannelObj>> channelDic = EpgDal.GetAllEpgChannelsDic(request.nGroupID);

            XmlNodeList xmlChannel = xmlDoc.GetElementsByTagName("TVGRIDBATCH");
            string sChannelID = Utils.GetSingleNodeValue(xmlChannel[0], "GN_ID");

            if (channelDic.ContainsKey(sChannelID))
            {
                foreach (EpgChannelObj channel in channelDic[sChannelID])
                {
                    int nChannelIDDB = channel.ChannelId;
                    if (channelID_DB_ALU.ContainsKey(nChannelIDDB))
                    {
                        int nChannelIDALU = channelID_DB_ALU[nChannelIDDB];
                        string sChannelName = channel.ChannelName;

                        // transform to XDTV and get response to send to ALU
                        res = TransformAndResponseALU(xmlDoc, nChannelIDALU, sChannelName);
                    }
                    else
                    {
                        log.Debug("InsertProgramsPerChannel - " + string.Format("no ALU channel ID found for channel {0}. channel cannot be sent to ALU", nChannelIDDB));
                    }
                }
            }

            log.Debug("Info - " + string.Concat("EPG XDTV transformed result: ", res));

            return res;
        }

        private string TransformAndResponseALU(XmlDocument XMLDoc, int channelIDALU, string sChannelName)
        {
            XmlDocument xmlResult = TransformToXDTV(XMLDoc, channelIDALU, sChannelName);

            return Utils.Compress(xmlResult.InnerXml);
        }

        private XmlDocument TransformToXDTV(XmlDocument XMLDoc, int channelIDALU, string sChannelName)
        {
            string xsltLoction = ApplicationConfiguration.GraceNoteXSLTPath.Value;

            XmlDocument xmlResult = new XmlDocument();
            GraceNoteTransform transformer = new GraceNoteTransform(xsltLoction);

            transformer.Init(channelIDALU, sChannelName);

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    transformer.Transform(XMLDoc, writer);

                    string utf8String = Utils.convertUTF16toUTF8(writer.ToString());

                    xmlResult.LoadXml(utf8String);
                }
                catch (Exception exp)
                {
                    log.Error("Error - " + string.Format("Exception in transforming xml from graceNote to ALU format", exp.Message), exp);
                    return null;
                }
                return xmlResult;
            }
        }


        private Dictionary<int, int> getALUIDs()
        {
            string sPath = ApplicationConfiguration.GraceNoteALUIdConvertion.Value;

            Dictionary<int, int> channelID_DB_ALU = null;
            if (File.Exists(sPath))
            {
                string json = File.ReadAllText(@sPath);
                channelID_DB_ALU = JsonConvert.DeserializeObject<Dictionary<int, int>>(json);
            }
            else
            {
                log.Debug("getALUIDs - " + string.Format("GraceNote_ALU_IDConvertion in path {0} was not found. channel cannot be sent to ALU", sPath));
            }

            return channelID_DB_ALU;
        }
    }
}
