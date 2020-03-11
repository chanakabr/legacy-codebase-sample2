using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.PlaybackAdapter
{
    public class RequestPlaybackContextOptions
    {        
        public string MediaProtocol { get; set; }
     
        public string StreamerType { get; set; }
     
        public string AssetFileIds { get; set; }
     
        public Dictionary<string, string> AdapterData { get; set; }
     
        public PlayContextType? Context { get; set; }
     
        public UrlType UrlType { get; set; }
       
        public string AssetId { get; set; }
       
        public eAssetTypes AssetType { get; set; }
    }
}