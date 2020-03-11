using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.DataLoaders
{
    using Tvinci.Data.TVMDataLoader;
    using Tvinci.Data.TVMDataLoader.Protocols;
    using Tvinci.Data.TVMDataLoader.Protocols.Search;
    using Tvinci.Data.DataLoader;

    [Serializable]
    public class SearchMediaIDLoader : TVMAdapter<TVPPro.SiteManager.DataLoaders.SearchMediaIDLoader.MediaIDTypePair>
    {
        [Serializable]
        public struct MediaIDTypePair
        {
            public long ID;
            public String SeriesName;
            public String Type;
        }

		#region Properties
		public String MediaName
		{
			get
			{
				return Parameters.GetParameter<String>(Tvinci.Data.DataLoader.eParameterType.Retrieve, "MediaName", null);
			}
			set
			{
				Parameters.SetParameter<String>(Tvinci.Data.DataLoader.eParameterType.Retrieve, "MediaName", value);
			}
		}

		public bool ExactSearch
		{
			get
			{
				return Parameters.GetParameter<bool>(eParameterType.Retrieve, "ExactSearch", false);
			}
			set
			{
				Parameters.SetParameter<bool>(eParameterType.Retrieve, "ExactSearch", value);
			}
		}

		public int? MediaType
		{
			get
			{
				return Parameters.GetParameter<int?>(eParameterType.Retrieve, "MediaType", null);
			}
			set
			{
				Parameters.SetParameter<int?>(eParameterType.Retrieve, "MediaType", value);
			}
		} 
		#endregion

        public SearchMediaIDLoader()
        {

        }

        protected override IProtocol CreateProtocol()
        {
            Tvinci.Data.TVMDataLoader.Protocols.Search.SearchProtocol protocol = new Tvinci.Data.TVMDataLoader.Protocols.Search.SearchProtocol();
            protocol.root.request.search_data.channel.start_index = "0";
            protocol.root.request.search_data.channel.media_count = "1";
            protocol.root.request.@params.with_info = "true";
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.statistics = true;
            protocol.root.request.search_data.cut_values.exact = ExactSearch;
            //protocol.root.request.type
            protocol.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = "Series name" });
            
			if (MediaType.HasValue)
                protocol.root.request.search_data.cut_values.type.value = (MediaType.Value).ToString();

            protocol.root.request.search_data.cut_values.name.value = MediaName;

            return protocol;
        }

        protected override MediaIDTypePair PreCacheHandling(object retrievedData)
        {
            MediaIDTypePair mediaPair = new MediaIDTypePair();
            
            if (retrievedData != null && retrievedData is SearchProtocol)
            {
                SearchProtocol result = retrievedData as SearchProtocol;
                if (result.response.channel.mediaCollection.Count > 0)
                {
                    mediaPair.ID = long.Parse(result.response.channel.mediaCollection[0].id);
                    mediaPair.SeriesName = (result.response.channel.mediaCollection[0].tags_collections.Count > 0)? result.response.channel.mediaCollection[0].tags_collections[0].tagCollection[0].name : String.Empty;
                    mediaPair.Type = result.response.channel.mediaCollection[0].type.value;
                }
            }

            return mediaPair;
        }

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{E7B81A95-8086-4d57-9995-CE4CEF094921}"); }
		}
    }
}