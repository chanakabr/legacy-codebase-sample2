using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Data.DataLoader;
using System.Data;
using System.Web;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using System.Text.RegularExpressions;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Site;
using TVPPro.Configuration.Technical;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
	public class TagSidebarLoader : CustomAdapter<TagSidebarLoader.LoaderResult>
	{
		public class LoaderResult
		{
			public Dictionary<string, string> Numbers { get; private set; }
			public Dictionary<string, string> Hebrew { get; private set; }
			public Dictionary<string, string> Rest { get; private set; }

			public LoaderResult()
			{
				Numbers = new Dictionary<string, string>();
				Hebrew = new Dictionary<string, string>();
				Rest = new Dictionary<string, string>();
			}
		}

		public bool MediaSearch
		{
			get
			{
				return Parameters.GetParameter<bool>(eParameterType.Retrieve, "MediaSearch", false);
			}
			set
			{
				Parameters.SetParameter<bool>(eParameterType.Retrieve, "MediaSearch", value);
			}
		}

		public string[] TagName
		{
			get
			{
				return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "TagName", null);
			}
			set
			{
				Parameters.SetParameter<string[]>(eParameterType.Retrieve, "TagName", value);
			}
		}

		public TagSidebarLoader(string[] tagName)
		{
			TagName = tagName;
		}

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{8EDDB127-3000-45fb-8F92-A555911DDD1C}"); }
		}

		protected override TagSidebarLoader.LoaderResult CreateSourceResult()
		{
            dsTags.ItemsDataTable table = new dsTags.ItemsDataTable();
            dsTags.ItemsDataTable tableTags = new TVMTagLoader(TagName).Execute();
            

            table.Merge(tableTags);

			if (MediaSearch)
			{
                dsTags.ItemsDataTable tableMovies = (new TVMMoviesLoader() { PageIndex = 0 }).Execute();
                table.Merge(tableMovies);
			}

			return createResult(table, table.NameColumn.ColumnName);
		}

		private TagSidebarLoader.LoaderResult createResult(DataTable table, string columnName)
		{
			TagSidebarLoader.LoaderResult result = new LoaderResult();
			
			if (table == null || table.Rows.Count == 0 || string.IsNullOrEmpty(columnName))
			{
				return result;
			}

			DataView dv = new DataView(table);
			dv.Sort = columnName;

			foreach (DataRowView row in dv)
			{
				string value = row[columnName] as string;

				if (!string.IsNullOrEmpty(value))
				{
					// format the value
					value = value.Trim();
					string firstLetter = value.Substring(0,1);

					if (Regex.IsMatch(firstLetter, "[א-ת]"))
					{
						result.Hebrew[value] = string.Empty;
					}
					else if (Regex.IsMatch(firstLetter, "[0-9]"))
					{
						result.Numbers[value] = string.Empty;
					} 
					else
					{
						result.Rest[value] = string.Empty;
					}					  
				}
			}
			
			return result;
		}

		[Serializable]
		private class TVMMoviesLoader : TVMAdapter<dsTags.ItemsDataTable>
		{
			public enum eMediaType
			{
				Movie,
				Episode,
				All
			}

			public eMediaType MediaType
			{
				get
				{
					return Parameters.GetParameter<eMediaType>(eParameterType.Retrieve, "MediaType", eMediaType.Movie);
				}
				set
				{
					Parameters.SetParameter<eMediaType>(eParameterType.Retrieve, "MediaType", value);
				}
			}

			public string OveriddenLanguage
			{
				get
				{
					return Parameters.GetParameter<string>(eParameterType.Retrieve, "Language", string.Empty);
				}
				set
				{
					Parameters.SetParameter<string>(eParameterType.Retrieve, "Language", value);
				}
			}

			public TVMMoviesLoader()
			{
				
			}

			protected override IProtocol CreateProtocol()
			{
				SearchProtocol protocol = new SearchProtocol();

				protocol.root.request.search_data.channel.start_index = (PageIndex * PageSize).ToString();
				protocol.root.request.search_data.channel.media_count = SiteConfiguration.Instance.Data.Features.MovieFinder.MaxItems.ToString();
				
				//SupportedItem item;
				//if (TechnicalConfiguration.Instance.TryGetItem(MediaType.ToString(), out item))
				//{
				//    protocol.root.request.search_data.cut_values.type.value = item.TVMInformation.TVMValue;

				//    protocol.root.request.@params.with_info = "false";
				//    protocol.root.request.@params.info_struct.statistics = false;
				//    protocol.root.request.@params.info_struct.MakeSchemaCompliant();
				//    protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();

				//    return protocol;
				//}
				//else
				//{
				//    return null;
				//}				

				return null;
			}

			protected override dsTags.ItemsDataTable PreCacheHandling(object retrievedData)
			{
				dsTags.ItemsDataTable result = new dsTags.ItemsDataTable();
				SearchProtocol data = (SearchProtocol) retrievedData;

				if (data != null)
				{
					if (data.response != null && data.response.channel.mediaCollection.Count > 0)
					{
						foreach (media media in data.response.channel.mediaCollection)
						{
							dsTags.ItemsRow row = result.NewItemsRow();
							row.Name = media.title;

							result.AddItemsRow(row);
						}
					}
				}

				return result;
			}

			protected override Guid UniqueIdentifier
			{
				get { return new Guid("{3B2CD41F-C795-42aa-88C5-4F12EF2C9862}"); }
			}
		}
	}
}
