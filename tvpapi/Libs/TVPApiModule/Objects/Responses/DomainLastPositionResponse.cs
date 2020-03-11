using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;


namespace TVPApiModule.Objects.Responses
{
    /// <summary>
    /// Summary description for DomainLastPositionResponse
    /// </summary>
    public class DomainLastPositionResponse
    {
        [JsonProperty(PropertyName = "m_lPositions")]
        public List<LastPosition> m_lPositions { get; set; }

        [JsonProperty(PropertyName = "m_sDescription")]
        public string m_sDescription { get; set; }

        [JsonProperty(PropertyName = "m_sStatus")]
        public string m_sStatus { get; set; }
    }

    /// <summary>
    /// Summary description for DomainLastPositionResponse
    /// </summary>    
    public class LastPosition
    {
        [JsonProperty(PropertyName = "m_nUserID")]
        public int m_nUserID;

        [JsonProperty(PropertyName = "m_eUserType")]
        public eUserType m_eUserType;

        [JsonProperty(PropertyName = "m_nLocation")]
        public int m_nLocation;

        public LastPosition()
        {
        }

        public LastPosition(Bookmark bookmark)
        {
            m_nUserID = int.Parse(bookmark.User.m_sSiteGUID);
            m_eUserType = bookmark.UserType;
            m_nLocation = bookmark.Location;
        }
    }
}