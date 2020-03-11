using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ApiObjects.Epg;
using Tvinci.Core.DAL;
using QueueWrapper;
using ApiObjects.MediaIndexingObjects;
using QueueWrapper.Queues.QueueObjects;
using ApiObjects;
using KLogMonitor;
using System.Reflection;


namespace GracenoteFeeder
{
    public class GracenoteFeederObj : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region CONST
        private const char spliter = '|';
        #endregion

        #region members
        public int GroupID { get; set; }
        public string Client { get; set; }
        public string User { get; set; }
        public string Language { get; set; }
        public string URL { get; set; }
        public string ChannelXml { get; set; }
        public string CategoryXml { get; set; }
        #endregion

        public GracenoteFeederObj(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            InitParamter();

        }


        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new GracenoteFeederObj(nTaskID, nIntervalInSec, sParameters);
        }

        /*
            1.Get All Channels from DB 
         *  2.Get the right XML from GraceNote Api by Channel_ID ()
         *  3. go for each XML and insert the program to all DB (sql, CB, ES)
         */
        protected override bool DoTheTaskInner()
        {
            try
            {
                //get all epg channel ids by group
                Dictionary<string, List<EpgChannelObj>> channelDic = EpgDal.GetAllEpgChannelsDic(GroupID);

                //GetAllChannels();
                List<string> channels = channelDic.Keys.ToList();
                if (channels != null && channels.Count == 0)
                {
                    log.Error("Error - " + string.Format("group:{0} No channels exists in DB for this group", GroupID));
                    return false;
                }

                // for each channel get the right xml file       
                List<RESPONSES> lResponse = getXmlTVChannel(channels);

                // run over and insert the programs
                bool res = InsertProgramsPerChannel(lResponse, channelDic);

                // Clear pic urls to support pic updates
                BaseGracenoteFeeder.dCategoryToDefaultPic.Clear();
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("group:{0}, ex:{1}", GroupID, ex.Message), ex);
            }

            return true;
        }


        //saves the EPGs and sends them to ALU
        private bool InsertProgramsPerChannel(List<RESPONSES> lResponse, Dictionary<string, List<EpgChannelObj>> channelDic)
        {
            try
            {
                List<XmlDocument> xmlList = getChannelXMLs(lResponse);

                #region send to celery Queue if needed
                try
                {
                    bool bSendToQueue = false;

                    string groupIDs = TVinciShared.WS_Utils.GetTcmConfigValue("graceNoteXDTVTransformGroups");
                    string[] sSep = { ";" };
                    string[] sGroupArray = groupIDs.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
                    if (sGroupArray.Contains(GroupID.ToString()))
                    {
                        bSendToQueue = true;
                    }

                    if (bSendToQueue)
                    {
                        foreach (XmlDocument xml in xmlList)
                        {
                            try
                            {
                                SendToQueue(xml);
                            }
                            catch (Exception exp)
                            {
                                log.Debug("SendToQueue in to loop - " + string.Format("failed to SendToQueue ex={0}", exp.Message));
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    log.Error("SendToQueue - " + string.Format("failed to SendToQueue ex={0}", ex.Message), ex);
                }
                #endregion

                //saving the channels in DB
                foreach (XmlDocument xml in xmlList)
                {
                    int nChannelIDDB = 0;
                    XmlNodeList xmlChannel = xml.GetElementsByTagName("TVGRIDBATCH");
                    string sChannelID = BaseGracenoteFeeder.GetSingleNodeValue(xmlChannel[0], "GN_ID");

                    if (channelDic.ContainsKey(sChannelID))
                    {
                        foreach (EpgChannelObj channel in channelDic[sChannelID])
                        {
                            nChannelIDDB = channel.ChannelId;

                            // Save epg programs for each xml documnet
                            SaveChannel(xml, nChannelIDDB, sChannelID, channel.ChannelType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertProgramsPerChannel -" + string.Format("Exception when proccing epg in group id {0}: {1}, at :{2}", GroupID, ex.Message, ex.StackTrace), ex);
                return false;
            }

            return true;
        }

        private void SaveChannel(XmlDocument xmlDoc, int nChannelID, string sChannelID, EpgChannelType eEpgChannelType)
        {
            try
            {
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(GroupID);
                BaseGracenoteFeeder gnf = new BaseGracenoteFeeder(Client, User, GroupID, URL, ChannelXml, CategoryXml, nParentGroupID, 700);
                if (gnf != null)
                {
                    gnf.SaveChannel(xmlDoc, nChannelID, sChannelID, eEpgChannelType);
                }
                else
                {
                    log.Debug("SaveChannels - " + string.Format("no implementation for groupid={0} can't save this channel", GroupID));
                }
            }

            catch (Exception ex)
            {
                log.Error("SaveChannels - " + string.Format("fail to create programObject by xml ex={0}", ex.Message), ex);
            }
        }

        // return true if response is ok and the url to get the xml from it 
        private bool IsStatusOK(RESPONSES response, out string sUrl)
        {
            sUrl = string.Empty;
            bool res = false;
            try
            {
                if (response != null && response.RESPONSE != null && response.RESPONSE.STATUS == "OK")
                {
                    if (response.RESPONSE.UPDATE_INFO != null && response.RESPONSE.UPDATE_INFO.URL != null)
                    {
                        sUrl = response.RESPONSE.UPDATE_INFO.URL.Value;
                        res = true;
                    }
                }
                else
                {
                    log.Debug("IsStatusOK - " + string.Format("Response is not OK  , responseStatus = {0},  responseURL = {1}",
                                       (response != null && response.RESPONSE != null) ? response.RESPONSE.STATUS : "response is null",
                                       (response != null && response.RESPONSE != null && response.RESPONSE.UPDATE_INFO != null && response.RESPONSE.UPDATE_INFO.URL != null) ?
                                       response.RESPONSE.UPDATE_INFO.URL.Value : "response url is null/empty") + " GracenoteFeeder");
                }
                return res;
            }
            catch (Exception ex)
            {
                log.Error("IsStatusOK - " + string.Format("Response is not OK  ex:{0} , responseStatus = {1},  responseURL = {2}", ex.Message,
                                            (response != null && response.RESPONSE != null) ? response.RESPONSE.STATUS : "response is null",
                                            (response != null && response.RESPONSE != null && response.RESPONSE.UPDATE_INFO != null && response.RESPONSE.UPDATE_INFO.URL != null) ?
                                            response.RESPONSE.UPDATE_INFO.URL.Value : "response url is null/empty"), ex);
                sUrl = string.Empty;
                return res;
            }
        }

        private void InitParamter()
        {
            try
            {
                string[] item = m_sParameters.Split(spliter);
                GroupID = ODBCWrapper.Utils.GetIntSafeVal(item[0]);
                Client = item[1];   //"11031808-0670AB37EBAF7858AABB6817516F992E";
                User = item[2];     // "262426818535595867-54FB85D9940A7501818E38904ECE5A55
                Language = item[3]; // DE
                URL = TVinciShared.WS_Utils.GetTcmConfigValue("UrlGN");  //"https://c11031808.ipg.web.cddbp.net/webapi/xml/1.0/tvgridbatch_update";
                ChannelXml = TVinciShared.WS_Utils.GetTcmConfigValue("ChannelXmlGN");
                CategoryXml = TVinciShared.WS_Utils.GetTcmConfigValue("CategoryXmlGN");
            }
            catch (Exception ex)
            {
                log.Error("InitParamter - " + string.Format("fail to split the parameters ex={0}", ex.Message), ex);
                throw ex;
            }
        }

        //Get all xml url by channelID from gracenote api service
        private List<RESPONSES> getXmlTVChannel(List<string> ChannelsID)
        {
            List<RESPONSES> lResponse = new List<RESPONSES>();
            try
            {
                foreach (string sChannelID in ChannelsID)
                {
                    string uri = URL;
                    string method = "POST";
                    string postData = string.Format(ChannelXml, Client, User, sChannelID, Language);
                    string sXml = Utils.getXmlFromGracenote(postData, method, uri);
                    if (string.IsNullOrEmpty(sXml))
                    {
                        log.Debug("getXmlTVChannel - " + string.Format("sXml is empty sChannelID = {0}, uri= {1}", sChannelID, uri));
                    }
                    lResponse.Add(ConvertXmlToResponseObj(sXml));
                }

                return lResponse;
            }
            catch (Exception ex)
            {
                log.Error("getXmlTVChannel - " + string.Format("ex={0}", ex.Message), ex);
                return new List<RESPONSES>();
            }
        }

        // convert the xml that we got from gracenote service to an object
        private RESPONSES ConvertXmlToResponseObj(string sXml)
        {
            RESPONSES oResponse = null;
            try
            {
                if (sXml != null)
                {
                    StringReader StrReader = new StringReader(sXml);
                    XmlSerializer Xml_Serializer = new XmlSerializer(typeof(RESPONSES));
                    XmlTextReader XmlReader = new XmlTextReader(StrReader);
                    try
                    {
                        oResponse = (RESPONSES)Xml_Serializer.Deserialize(XmlReader);
                    }
                    catch (Exception ex)
                    {
                        log.Error("ConvertXmlToResponseObj - " + string.Format("fail to convert the xml to Response Object, ex:{0}", ex.Message), ex);
                        oResponse = null;
                    }
                }

                return oResponse;
            }
            catch (Exception ex)
            {
                log.Error("ConvertXmlToResponseObj - " + string.Format("fail to convert the xml to Response Object, ex:{0}", ex.Message), ex);
                return null;
            }
        }

        private List<XmlDocument> getChannelXMLs(List<RESPONSES> lResponse)
        {
            List<XmlDocument> xmlList = new List<XmlDocument>();

            foreach (RESPONSES response in lResponse)
            {
                try
                {
                    string sUrl = string.Empty;
                    XmlDocument xmlDoc = null;
                    if (IsStatusOK(response, out sUrl))
                    {
                        try
                        {
                            string sXml = Utils.getXmlFromGracenote(string.Empty, "GET", sUrl);

                            xmlDoc = new XmlDocument();
                            Encoding encoding = Encoding.UTF8;

                            // Encode the XML string in a byte array
                            byte[] encodedString = encoding.GetBytes(sXml);

                            // Put the byte array into a stream and rewind it to the beginning
                            using (var ms = new MemoryStream(encodedString))
                            {
                                ms.Flush();
                                ms.Position = 0;

                                // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                                xmlDoc.Load(ms);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("getChannelXMLs - " + string.Format("sURL:{0}, ex:{1}", sUrl, ex.Message) + " FailGetURL", ex);
                        }

                        if (xmlDoc != null)
                        {
                            xmlList.Add(xmlDoc);
                        }
                    }
                    else
                    {
                        log.Debug("getChannelXMLs - " + string.Format("Response has error sUrl:{0}", sUrl));
                    }
                }
                catch (Exception exp) // if one request failed don't fail it all
                {
                    log.Error("getChannelXMLs - " + string.Format("xml:{0}, ex:{1}", GroupID, exp.Message), exp);
                }
            }
            return xmlList;
        }

        private void SendToQueue(XmlDocument XMLDoc)
        {
            List<object> args = new List<object>();
            string id = Guid.NewGuid().ToString();
            string task = TVinciShared.WS_Utils.GetTcmConfigValue("taskEPG");
            string sRoutingKey = TVinciShared.WS_Utils.GetTcmConfigValue("routingKeyEPG");
            string compressedXML = Utils.Compress(XMLDoc.InnerXml);
            string sUrlALUCheck = TVinciShared.WS_Utils.GetTcmConfigValue("alcatelLucentHostCheck");
            string sUrlALUSend = TVinciShared.WS_Utils.GetTcmConfigValue("alcatelLucentHostSend");

            XmlNodeList xmlChannel = XMLDoc.GetElementsByTagName("TVGRIDBATCH");
            string sChannelExternalID = BaseGracenoteFeeder.GetSingleNodeValue(xmlChannel[0], "GN_ID");

            args.Add(GroupID);
            args.Add(sUrlALUCheck);
            args.Add(sUrlALUSend);
            args.Add(compressedXML);
            //args.Add(sChannelExternalID);//this is the "Channel_ID column in epg_channels table

            BaseCeleryData data = new BaseCeleryData(id, task, args);
            data.GroupId = this.GroupID;
            BaseQueue queue = new EPGQueue();

            bool bIsUpdateSucceeded = queue.Enqueue(data, sRoutingKey);
            if (!bIsUpdateSucceeded)
            {
                log.Error("Error - EPGQueue was not updated  - xml was not sent to ALU. GraceNoteFeeder");
            }
        }
    }
}
