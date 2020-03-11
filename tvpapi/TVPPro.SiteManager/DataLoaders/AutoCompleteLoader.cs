using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using TVPPro.Configuration.Site;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataEntities;
using System.Data;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class AutoCompleteLoader : TVMAdapter<List<String>>
    {
        public string[] MetaNames
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "MetaNames", null);
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "MetaNames", value);
            }
        }

        public int? MediaTypeID
        {
            get
            {
                return Parameters.GetParameter<int?>(eParameterType.Retrieve, "MediaTypeID", null);
            }
            set
            {
                Parameters.SetParameter<int?>(eParameterType.Retrieve, "MediaTypeID", value);
            }
        }

        public AutoCompleteLoader()
        {
            
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SearchProtocol protocol = new SearchProtocol();

            protocol.root.request.search_data.channel.start_index = "0";
            protocol.root.request.search_data.channel.media_count = SiteConfiguration.Instance.Data.Features.MovieFinder.MaxItems.ToString();
            
            protocol.root.request.@params.with_info = "true";
            //protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();

            if (MediaTypeID.HasValue)
                protocol.root.request.search_data.cut_values.type.value = MediaTypeID.ToString();

            foreach (string key in MetaNames)
            {
                protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = key });
            }
            
            return protocol;
        }

        protected override List<String> PreCacheHandling(object retrievedData)
        {
            List<String> lstRet = new List<String>();
            List<String> lstMetas = new List<String>();

            SearchProtocol result = (retrievedData as SearchProtocol);

            if (result != null)
            {
                dsItemInfo dsItems = new dsItemInfo();

                foreach (media resMedia in result.response.channel.mediaCollection)
                {
                    lstRet.Add(resMedia.title.ToString());
                    TVPPro.SiteManager.Helper.DataHelper.CollectMetasInfo(ref dsItems, resMedia);
                }

                foreach (DataColumn column in dsItems.Metas.Columns)
                {
                    if (!column.ColumnName.ToLower().Equals("id")) lstRet.AddRange(dsItems.Metas.Select(r => r[column].ToString()).ToArray());
                }

                // remove empty and duplicates
                lstRet.Remove("");
                lstRet = lstRet.Distinct().ToList();
            }

            return lstRet;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{AE3FFCC1-7881-47fb-909F-3AD25D9D98CE}"); }
        }
    }
}
