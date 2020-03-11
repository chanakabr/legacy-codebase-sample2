using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for UsersXMLParser
/// </summary>
/// 

namespace TVPApi
{
    public class UsersXMLParser
    {

        private Dictionary<string, Platform> m_platforms = new Dictionary<string, Platform>();
        private static UsersXMLParser m_instance;

        public UsersXMLParser()
        {
            LoadUsersXml();
        }

        public static UsersXMLParser Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new UsersXMLParser();
                }
                return m_instance;
            }
        }

        public static void Clear()
        {
            m_instance = null;
        }

        public string GetGuid(string platformStr, string sID)
        {
            string retVal = string.Empty;
            if (m_platforms.ContainsKey(platformStr))
            {
                Platform platform = m_platforms[platformStr];
                if (platform.Devices.ContainsKey(sID))
                {
                    Device device = platform.Devices[sID];
                    retVal = device.GUID;
                }
            }
            return retVal;
        }

        private void LoadUsersXml()
        {
            string FileLocation = string.Format(@"~/XML/{0}", "Users.xml");

            XmlDocument usersXml = new XmlDocument();
            TVPPro.SiteManager.Helper.SiteHelper.TryLoadXML(FileLocation, out usersXml);

            XmlNodeList NodeList = usersXml.SelectNodes("Platforms/platform");
            foreach (XmlNode node in NodeList)
            {
                string platfromName = node.SelectSingleNode("name").InnerText;
                m_platforms.Add(platfromName, new Platform(node));
            }

        }


        public struct Device
        {
            string deviceID;
            string guid;

            public Device(string deviceID, string guid)
            {
                this.deviceID = deviceID;
                this.guid = guid;
            }

            public string DeviceID
            {
                get
                {
                    return deviceID;
                }
            }

            public string GUID
            {
                get
                {
                    return guid;
                }
            }
        }

        public struct Platform
        {
            string platformName;
            Dictionary<string, Device> m_devices;

            public Platform(XmlNode node)
            {
                m_devices = new Dictionary<string, Device>();
                platformName = node.SelectSingleNode("name").InnerText;
                XmlNodeList nodeList = node.SelectNodes("deviceids/deviceid");
                foreach (XmlNode deviceNode in nodeList)
                {
                    string deviceID = deviceNode.Attributes["id"].Value;
                    string guid = deviceNode.Attributes["guid"].Value;
                    m_devices.Add(deviceID, new Device(deviceID, guid));
                }
            }

            public Dictionary<string, Device> Devices
            {
                get
                {
                    return m_devices;
                }
            }

            public string PlatfromName
            {
                get
                {
                    return platformName;
                }

            }

        }
    }
}
