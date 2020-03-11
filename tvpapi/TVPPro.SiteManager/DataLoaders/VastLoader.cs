using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.VastProtocol;
using Tvinci.Data.DataLoader;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class VastLoader : TVMAdapter<string[]>
    {
        private string m_tvmUser;
        private string m_tvmPass;
        private int m_width;
        private int m_height;
        public enum eAssetType { Media, Page }

        #region properties
        public string AssetID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "AssetID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "AssetID", value);
            }
        }

        public eAssetType AssetType
        {
            get
            {
                return Parameters.GetParameter<eAssetType>(eParameterType.Retrieve, "AssetType", eAssetType.Media);
            }
            set
            {
                Parameters.SetParameter<eAssetType>(eParameterType.Retrieve, "AssetType", value);
            }
        }

        #endregion

        public VastLoader(string user, string pass, int width, int height, string assetID, eAssetType assetType)
        {
            ((TVMProvider)GetProvider()).TVMAltURL = "http://173.231.146.34:9003/platform-us/vast_gateway.aspx";

            m_tvmUser = user;
            m_tvmPass = pass;
            m_width = width;
            m_height = height;
            AssetID = assetID;
            AssetType = assetType;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            VastProtocol protocol = new VastProtocol();
            protocol.root.flashvars.player_un = m_tvmUser;
            protocol.root.flashvars.player_pass = m_tvmPass;
            protocol.root.flashvars.no_cache = "0";
            protocol.root.flashvars.pic_size1 = "full";
            protocol.root.flashvars.file_format = "iPad Main";
            protocol.root.flashvars.file_quality = "high";
            protocol.root.flashvars.lang = string.Empty;

            protocol.root.request.@params.height = m_height.ToString();
            protocol.root.request.@params.width = m_width.ToString();
            protocol.root.request.@params.id = AssetID;
            protocol.root.request.@params.type = AssetType == eAssetType.Media ? type.media : type.page;

            return protocol;
        }

        protected override string[] PreCacheHandling(object retrievedData)
        {
            return new string[] { (retrievedData as VastProtocol).response.companionAd.Value, (retrievedData as VastProtocol).response.companionAd.clickthrough, 
                (retrievedData as VastProtocol).response.companionAd.creativeView };
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{5DC3DE21-1B2A-47B7-BFB6-9F682902B985}"); }
        }
    }
}
