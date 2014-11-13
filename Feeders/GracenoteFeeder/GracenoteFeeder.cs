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

namespace GracenoteFeeder
{
    public class GracenoteFeederObj : ScheduledTasks.BaseTask
    {
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
                //call catalog to get all epg channel ids by group
                List<string> channels = GetAllChannels();
                if (channels != null && channels.Count == 0)
                {
                    Logger.Logger.Log("Error", string.Format("group:{0} No channels exsits in DB for this group", GroupID), "GracenoteFeeder");
                    return false;
                }

                // foreach channel get the right xml file       
                List<RESPONSES> lResponse = getXmlTVChannel(channels);

                // run over and insert the programs
                bool res = InsertProgramsPerChannel(lResponse);
                
                // Clear pic urls to support pic updates
                BaseGracenoteFeeder.dCategoryToDefaultPic.Clear();
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("group:{0}, ex:{1}", GroupID, ex.Message), "GracenoteFeeder");
            }

            return true;
        }


        //saves the EPGs and sends them to ALU
        private bool InsertProgramsPerChannel(List<RESPONSES> lResponse)
        {
            try
            {
                List<XmlDocument> xmlList = getChannelXMLs(lResponse);

                foreach (XmlDocument xml in xmlList)
                {
                    //send to ALU
                    TransformAndSendToALU(xml);

                    // Save epg programs for each xml documnet
                    SaveChannel(xml);
                }
            }
            catch (Exception ex)
            {
                
                return false;
            }
            return true;
        }

        private void SaveChannel(XmlDocument xmlDoc)
        {   
            try
            {
                 int nParentGroupID = DAL.UtilsDal.GetParentGroupID(GroupID);
                 BaseGracenoteFeeder gnf = new BaseGracenoteFeeder(Client, User, GroupID, URL, ChannelXml, CategoryXml,nParentGroupID, 700);
                 if (gnf != null)
                 {
                     gnf.SaveChannel(xmlDoc);
                 }
                 else
                 {
                     Logger.Logger.Log("SaveChannels", string.Format("no implementation for groupid={0} can't save this channel", GroupID), "GracenoteFeeder");
                 }               
            }

            catch (Exception ex)
            {
                Logger.Logger.Log("SaveChannels", string.Format("fail to create programObject by xml ex={0}", ex.Message), "GracenoteFeeder");
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
                    Logger.Logger.Log("IsStatusOK", string.Format("Response is not OK  , responseStatus = {0},  responseURL = {1}", 
                                       (response != null && response.RESPONSE != null) ? response.RESPONSE.STATUS : "response is null",
                                       (response != null && response.RESPONSE != null && response.RESPONSE.UPDATE_INFO != null && response.RESPONSE.UPDATE_INFO.URL != null) ?
                                       response.RESPONSE.UPDATE_INFO.URL.Value : "response url is null/empty"), "GracenoteFeeder");
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("IsStatusOK", string.Format("Response is not OK  ex:{0} , responseStatus = {1},  responseURL = {2}", ex.Message,
                                            (response != null && response.RESPONSE != null) ? response.RESPONSE.STATUS : "response is null",
                                            (response != null && response.RESPONSE != null && response.RESPONSE.UPDATE_INFO != null && response.RESPONSE.UPDATE_INFO.URL != null) ?
                                            response.RESPONSE.UPDATE_INFO.URL.Value : "response url is null/empty"), "GracenoteFeeder");
                sUrl = string.Empty;
                return res;
            }
        }

        // get all channels by group id from DB
        private List<string> GetAllChannels()
        {
            try
            {
                List<string> channels = new List<string>();
                DataTable dt = EpgDal.GetAllEpgChannelsList(GroupID);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string channelId = ODBCWrapper.Utils.GetSafeStr(row, "CHANNEL_ID").Replace("\r", "").Replace("\n", "");
                        channels.Add(channelId);
                    }
                }
                return channels;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetAllChannels", string.Format("faild to get channels for group:{0}, ex:{1}", GroupID, ex.Message), "GracenoteFeeder");
                return new List<string>();
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
                Logger.Logger.Log("InitParamter", string.Format("fail to split the parameters ex={0}", ex.Message), "GracenoteFeeder");
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
                        Logger.Logger.Log("getXmlTVChannel", string.Format("sXml is empty sChannelID = {0}, uri= {1}", sChannelID, uri), "GracenoteFeeder");
                    }
                    lResponse.Add(ConvertXmlToResponseObj(sXml));
                }

                return lResponse;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("getXmlTVChannel", string.Format("ex={0}", ex.Message), "GracenoteFeeder");
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
                        Logger.Logger.Log("ConvertXmlToResponseObj", string.Format("fail to convert the xml to Response Object, ex:{0}", ex.Message), "GracenoteFeeder");
                        oResponse = null;
                    }
                }        
         
                return oResponse;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("ConvertXmlToResponseObj", string.Format("fail to convert the xml to Response Object, ex:{0}", ex.Message), "GracenoteFeeder");
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
                            #region Load the xml string to XmlDocument
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
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("getChannelXMLs", string.Format("sURL:{0}, ex:{1}", sUrl, ex.Message), "FailGetURL");
                        }

                        if (xmlDoc != null)
                        {
                            xmlList.Add(xmlDoc);
                        }
                    }
                    else
                    {
                        Logger.Logger.Log("getChannelXMLs", string.Format("Response has error sUrl:{0}", sUrl), "FailGetURL");
                    }
                }
                catch (Exception exp) // if one request faild don't fail it all
                {
                    Logger.Logger.Log("getChannelXMLs", string.Format("xml:{0}, ex:{1}", GroupID, exp.Message), "GracenoteFeeder");
                }
            }
            return xmlList;
        }

        private void TransformAndSendToALU(XmlDocument XMLDoc)
        {
            XmlDocument xmlResult = TransformToALU(XMLDoc);

            SendToALU(XMLDoc);          
        }

        private XmlDocument TransformToALU(XmlDocument XMLDoc)
        {
            XmlDocument xmlResult = new XmlDocument();
            GraceNoteTransform transformer = new GraceNoteTransform();
            transformer.Init();
            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    transformer.Transform(XMLDoc, writer);
                    xmlResult.LoadXml(writer.ToString());
                }
                catch (Exception exp)
                {
                    Logger.Logger.Log("Error", string.Format("Exception in transforming xml from graceNote to ALU format", exp.Message), "GraceNoteFeeder");
                    return null;
                }
                return xmlResult;
            }


        }

        //TODO 
        private void SendToALU(XmlDocument XMLDoc)
        {


        }
    }
}
