using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADIFeeder
{
    public class ADIMapping
    {
        public delegate void SetParameter(ADIFeeder thisParameter, string sValue, string key);

        public struct KeyAndMethod
        {
            public KeyAndMethod(string key, SetParameter method)
            {
                m_key = key;
                m_method = method;
            }

            public string m_key;
            public SetParameter m_method;
        };

        public Dictionary<string, KeyAndMethod> lMapItems = new Dictionary<string, KeyAndMethod>() {   
                                                        {"licensing_window_start",  new KeyAndMethod("",                          ADIMethods.HLicensingWindowStart) } ,
                                                        {"show_type",               new KeyAndMethod("",                          ADIMethods.HShowType) } ,
                                                        {"licensing_window_end",    new KeyAndMethod("",                          ADIMethods.HLicensingWindowEnd) } ,
                                                        {"purge_date",              new KeyAndMethod("",                          ADIMethods.HPurgeDate) } ,
                                                        {"title",                   new KeyAndMethod("name",                      ADIMethods.HInsertBasic) } ,
                                                        {"run_time",                new KeyAndMethod("",                          ADIMethods.HRunTime) } ,
                                                        {"summary_long",            new KeyAndMethod("description",               ADIMethods.HInsertBasic) } ,
                                                        {"summary_medium",          new KeyAndMethod("Short summary",             ADIMethods.HInsertString) } ,
                                                        {"short_pinyin",            new KeyAndMethod("Short Pinyin title",        ADIMethods.HInsertString) } ,
                                                        {"long_pinyin",             new KeyAndMethod("Long Pinyin title",         ADIMethods.HInsertString) } ,
                                                        {"url",                     new KeyAndMethod("URL",                       ADIMethods.HInsertString) } ,
                                                        {"title_brief",             new KeyAndMethod("Short title",               ADIMethods.HInsertString) } ,
                                                        {"year",                    new KeyAndMethod("Release year",              ADIMethods.HInsertNum) } ,
                                                        {"billing_id",              new KeyAndMethod("",                          ADIMethods.HBillingID) } ,
                                                        {"episode_id",              new KeyAndMethod("",                          ADIMethods.HEpisodeId) } ,
                                                        {"episode_name",            new KeyAndMethod("Episode name",              ADIMethods.HInsertString) } ,
                                                        {"seasonnumber",            new KeyAndMethod("",                          ADIMethods.HSeasonNumber) } ,
                                                        {"season_premiere",         new KeyAndMethod("Season premiere",           ADIMethods.HInsertBool) } ,
                                                        {"season_finale",           new KeyAndMethod("Season finale",             ADIMethods.HInsertBool) } ,
                                                        {"closed_captions_available", new KeyAndMethod("Closed captions available", ADIMethods.HInsertBool) } ,
                                                        {"interactive",             new KeyAndMethod("interactive",               ADIMethods.HInsertBool) } ,
                                                        {"seasonpackageassetid",    new KeyAndMethod("SeasonPackageAssetID",      ADIMethods.HInsertString) } ,
                                                        {"ios_product",             new KeyAndMethod("Product Code",               ADIMethods.HInsertString) } ,
                                                        {"dtw_product",             new KeyAndMethod("DTW Product",               ADIMethods.HInsertString) } ,
                                                        {"dtw_billing_code",        new KeyAndMethod("DTW Billing Code",          ADIMethods.HInsertString) } ,
                                                        {"tx_date",                 new KeyAndMethod("TX Date",                   ADIMethods.HTXDate)     } ,
                                                        {"rating",                  new KeyAndMethod("Rating",                    ADIMethods.HInsertTags) } ,
                                                        {"genre",                   new KeyAndMethod("Genre",                     ADIMethods.HInsertTags) } ,
                                                        {"broadcaster",             new KeyAndMethod("Broadcaster",               ADIMethods.HInsertTags) } ,
                                                        {"featured_channel",        new KeyAndMethod("Featured Channel",          ADIMethods.HInsertTags) } ,
                                                        {"school_level",            new KeyAndMethod("School Level",              ADIMethods.HInsertTags) } ,
                                                        {"school_subject",          new KeyAndMethod("School Subject",            ADIMethods.HInsertTags) } ,
                                                        {"school_chapter",          new KeyAndMethod("Chapter",                   ADIMethods.HInsertTags) } ,
                                                        {"search_tag",              new KeyAndMethod("Search Tag",                ADIMethods.HInsertTags) } ,
                                                        {"extra_type",              new KeyAndMethod("Extra Type",                ADIMethods.HInsertTags) } ,
                                                        {"extra_virtual_media_link",new KeyAndMethod("Extra Virtual Media Link",  ADIMethods.HInsertTags) } ,
                                                        {"extra_regular_media_link",new KeyAndMethod("Extra Regular Media Link",  ADIMethods.HInsertTags) } ,
                                                        {"subject_tag",             new KeyAndMethod("Subject Tag",               ADIMethods.HInsertTags) } ,
                                                        {"featured_organisation",   new KeyAndMethod("Featured Organisation",     ADIMethods.HInsertTags) } ,
                                                        {"featured_individual",     new KeyAndMethod("Featured Individual",       ADIMethods.HInsertTags) } ,
                                                        {"seriesid",                new KeyAndMethod("Series name",               ADIMethods.HInsertTags) } ,
                                                        {"actors_display",          new KeyAndMethod("Main cast",                 ADIMethods.HInsertTags) } ,
                                                        {"category",                new KeyAndMethod("Category",                  ADIMethods.HInsertTags) } ,
                                                        {"advisories",              new KeyAndMethod("Rating advisories",         ADIMethods.HInsertTags) } ,
                                                        {"director",                new KeyAndMethod("Director",                  ADIMethods.HInsertTags) } ,
                                                        {"territory",               new KeyAndMethod("Territory",                 ADIMethods.HInsertTags) } ,
                                                        {"ad_tag_1",                new KeyAndMethod("Ad tag 1",                  ADIMethods.HInsertTags) } ,
                                                        {"ad_tag_2",                new KeyAndMethod("Ad tag 2",                  ADIMethods.HInsertTags) } ,
                                                        {"ad_tag_3",                new KeyAndMethod("Ad tag 3",                  ADIMethods.HInsertTags) } ,
                                                        {"hash_tag",                new KeyAndMethod("Hashtag",                   ADIMethods.HInsertString) } ,
                                                        {"ad_break",                new KeyAndMethod("",                          ADIMethods.HAdBreak)      } ,
                                                        {"geo_block_rule",          new KeyAndMethod("",                          ADIMethods.HGeoBlockRule) } ,
                                                        {"device_rule",             new KeyAndMethod("",                          ADIMethods.HDeviceRule)   } ,
                                                        {"asset_is_active",         new KeyAndMethod("Active",                    ADIMethods.HInsertBasic)   } ,
                                                        {"free",                    new KeyAndMethod("Free",                      ADIMethods.HInsertTags) } ,
                                                        {"eshop_url",               new KeyAndMethod("Eshop URL",                 ADIMethods.HInsertString) } ,
                                                        {"total_number_eps",        new KeyAndMethod("Total Number of Episodes",  ADIMethods.HInsertNum) } };
    }
}
