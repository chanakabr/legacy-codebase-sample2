using ApiObjects;
using ApiObjects.SearchObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupsCacheManager;

namespace ElasticSearch.Common
{
    public class ESSerializerV2 : BaseESSeralizer
    {
        /*
         * http://www.joda.org/joda-time/apidocs/org/joda/time/format/DateTimeFormat.html
 ------  -------                      ------------  -------
 G       era                          text          AD
 C       century of era (>=0)         number        20
 Y       year of era (>=0)            year          1996

 x       weekyear                     year          1996
 w       week of weekyear             number        27
 e       day of week                  number        2
 E       day of week                  text          Tuesday; Tue

 y       year                         year          1996
 D       day of year                  number        189
 M       month of year                month         July; Jul; 07
 d       day of month                 number        10

 a       halfday of day               text          PM
 K       hour of halfday (0~11)       number        0
 h       clockhour of halfday (1~12)  number        12

 H       hour of day (0~23)           number        0
 k       clockhour of day (1~24)      number        24
 m       minute of hour               number        30
 s       second of minute             number        55
 S       fraction of second           millis        978

 z       time zone                    text          Pacific Standard Time; PST
 Z       time zone offset/id          zone          -0800; -08:00; America/Los_Angeles

 '       escape for text              delimiter
 ''      single quote                 literal       '
 
         */

        protected readonly string DATE_FORMAT = "yyyyMMddHHmmss";
        protected const string LOWERCASE_ANALYZER = "lowercase_analyzer";
        protected const string PHRASE_STARTS_WITH_ANALYZER = "phrase_starts_with_analyzer";
        protected const string PHRASE_STARTS_WITH_SEARCH_ANALYZER = "phrase_starts_with_search_analyzer";
        protected const string PHRASE_STARTS_WITH_FILTER = "edgengram_filter";

        public ESSerializerV2()
        {
            shouldLowerCase = false;
        }

        /// <summary>
        /// Read things like this:
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/breaking_20_mapping_changes.html
        /// and this:
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping.html
        /// </summary>
        /// <param name="autocompleteSearchAnalyzer"></param>
        /// <returns></returns>
        public override string CreateMediaMapping(Dictionary<string, KeyValuePair<eESFieldType, string>> metasMap, List<string> groupTags,
            MappingAnalyzers specificLanguageAnalyzers, MappingAnalyzers defaultLanguageAnalyzers)
        {
            string normalIndexAnalyzer = specificLanguageAnalyzers.normalIndexAnalyzer;
            string normalSearchAnalyzer = specificLanguageAnalyzers.normalSearchAnalyzer;
            string autocompleteIndexAnalyzer = specificLanguageAnalyzers.autocompleteIndexAnalyzer;
            string autocompleteSearchAnalyzer = specificLanguageAnalyzers.autocompleteSearchAnalyzer;
            string suffix = specificLanguageAnalyzers.suffix;
            string phoneticIndexAnalyzer = specificLanguageAnalyzers.phoneticIndexAnalyzer;
            string phoneticSearchAnalyzer = specificLanguageAnalyzers.phoneticSearchAnalyzer;

            string defaultNormalIndexAnalyzer = normalIndexAnalyzer;
            string defaultNormalSearchAnalyzer = normalSearchAnalyzer;
            string defaultAutocompleteIndexAnalyzer = autocompleteIndexAnalyzer;
            string defaultAutocompleteSearchAnalyzer = autocompleteSearchAnalyzer;
            string defaultPhoneticIndexAnalyzer = phoneticIndexAnalyzer;
            string defaultPhoneticSearchAnalyzer = phoneticSearchAnalyzer;

            if (defaultLanguageAnalyzers != null)
            {
                defaultNormalIndexAnalyzer = defaultLanguageAnalyzers.normalIndexAnalyzer;
                defaultNormalSearchAnalyzer = defaultLanguageAnalyzers.normalSearchAnalyzer;
                defaultAutocompleteIndexAnalyzer = defaultLanguageAnalyzers.autocompleteIndexAnalyzer;
                defaultAutocompleteSearchAnalyzer = defaultLanguageAnalyzers.autocompleteSearchAnalyzer;
                defaultPhoneticIndexAnalyzer = defaultLanguageAnalyzers.phoneticIndexAnalyzer;
                defaultPhoneticSearchAnalyzer = defaultLanguageAnalyzers.phoneticSearchAnalyzer;
            }

            if (metasMap == null || groupTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj(AddSuffix("media", suffix));

            #region Add basic type mappings - (e.g. media_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "media_id",
                type = eESFieldType.LONG,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "group_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "media_type_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "epg_channel_id",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "wp_type_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "is_active",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "device_rule_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "like_counter",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "start_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "catalog_start_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "end_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "final_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "update_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "cache_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "user_types",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed
            });

            ElasticSearch.Common.FieldsMappingPropertyV2 nameProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                nameProperty.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = autocompleteSearchAnalyzer,
                    analyzer = autocompleteIndexAnalyzer
                });
            }

            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
            {
                nameProperty.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "phonetic",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = phoneticSearchAnalyzer,
                    analyzer = phoneticIndexAnalyzer
                });
            }

            mappingObj.AddProperty(nameProperty);

            ElasticSearch.Common.FieldsMappingPropertyV2 descProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };

            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
            });
            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            descProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            descProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = autocompleteSearchAnalyzer,
                    analyzer = autocompleteIndexAnalyzer
                });
            }

            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
            {
                descProperty.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "phonetic",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = phoneticSearchAnalyzer,
                    analyzer = phoneticIndexAnalyzer
                });
            }

            mappingObj.AddProperty(descProperty);

            ElasticSearch.Common.FieldsMappingPropertyV2 externalId = new FieldsMappingPropertyV2()
            {
                name = "external_id",
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            externalId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "external_id",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            externalId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = defaultNormalSearchAnalyzer,
                analyzer = defaultNormalIndexAnalyzer
            });
            externalId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            externalId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                externalId.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = defaultAutocompleteSearchAnalyzer,
                    analyzer = defaultAutocompleteIndexAnalyzer
                });
            }

            mappingObj.AddProperty(externalId);

            ElasticSearch.Common.FieldsMappingPropertyV2 entryId = new FieldsMappingPropertyV2()
            {
                name = "entry_id",
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            entryId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "entry_id",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            entryId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = defaultNormalSearchAnalyzer,
                analyzer = defaultNormalIndexAnalyzer
            });
            entryId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            entryId.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                entryId.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = defaultAutocompleteSearchAnalyzer,
                    analyzer = defaultAutocompleteIndexAnalyzer
                });
            }

            mappingObj.AddProperty(entryId);

            #endregion

            #region Add tags mapping
            InnerMappingPropertyV2 tags = new InnerMappingPropertyV2()
            {
                name = "tags"
            };

            if (groupTags.Count > 0)
            {
                HashSet<string> mappedTags = new HashSet<string>();

                foreach (string tagName in groupTags)
                {
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        string loweredTagName = tagName.ToLower();

                        // Don't create double mapping for tags to avoid errors
                        if (mappedTags.Contains(loweredTagName))
                        {
                            continue;
                        }

                        FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                        {
                            name = AddSuffix(loweredTagName, suffix),
                            type = eESFieldType.STRING,
                            index = eMappingIndex.analyzed,
                            search_analyzer = LOWERCASE_ANALYZER,
                            analyzer = LOWERCASE_ANALYZER,
                            null_value = ""
                        };
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                        {
                            name = AddSuffix(loweredTagName, suffix),
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = string.Empty,
                            index = eMappingIndex.analyzed,
                            search_analyzer = LOWERCASE_ANALYZER,
                            analyzer = LOWERCASE_ANALYZER,
                        });
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                        {
                            name = "analyzed",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            index = eMappingIndex.analyzed,
                            search_analyzer = normalSearchAnalyzer,
                            analyzer = normalIndexAnalyzer
                        });
                        multiField.fields.Add(new BasicMappingPropertyV2()
                        {
                            name = "lowercase",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            index = eMappingIndex.analyzed,
                            search_analyzer = LOWERCASE_ANALYZER,
                            analyzer = LOWERCASE_ANALYZER
                        });
                        multiField.fields.Add(new BasicMappingPropertyV2()
                        {
                            name = "phrase_autocomplete",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            index = eMappingIndex.analyzed,
                            search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                            analyzer = PHRASE_STARTS_WITH_ANALYZER
                        });

                        if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                        {
                            multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
                            {
                                name = "autocomplete",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = autocompleteSearchAnalyzer,
                                analyzer = autocompleteIndexAnalyzer
                            });
                        }

                        if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
                        {
                            multiField.fields.Add(new BasicMappingPropertyV2()
                            {
                                name = "phonetic",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = phoneticSearchAnalyzer,
                                analyzer = phoneticIndexAnalyzer
                            });
                        }

                        tags.AddProperty(multiField);
                        mappedTags.Add(loweredTagName);
                    }
                }
            }

            #endregion

            #region Add metas mapping

            InnerMappingPropertyV2 metas = new InnerMappingPropertyV2()
            {
                name = "metas"
            };

            if (metasMap != null && metasMap.Count > 0)
            {
                HashSet<string> mappedMetas = new HashSet<string>();
                foreach (KeyValuePair<string, KeyValuePair<eESFieldType, string>> meta in metasMap)
                {
                    string sMetaName = meta.Key;
                    if (!string.IsNullOrEmpty(sMetaName))
                    {
                        sMetaName = sMetaName.ToLower();
                        // Don't create double mapping for metas to avoid errors
                        if (mappedMetas.Contains(sMetaName))
                        {
                            continue;
                        }
                        if (meta.Value.Key != eESFieldType.DATE)
                        {

                            FieldsMappingPropertyV2 multiField = null;

                            if (meta.Value.Key == eESFieldType.STRING)
                            {
                                multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = LOWERCASE_ANALYZER,
                                    analyzer = LOWERCASE_ANALYZER,
                                    null_value = meta.Value.Value
                                };

                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    null_value = meta.Value.Value,
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = LOWERCASE_ANALYZER,
                                    analyzer = LOWERCASE_ANALYZER,
                                });

                                multiField.fields.Add(new BasicMappingPropertyV2()
                                {
                                    name = "phrase_autocomplete",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                                    analyzer = PHRASE_STARTS_WITH_ANALYZER
                                });
                            }
                            else
                            {
                                multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    index = eMappingIndex.not_analyzed,
                                    null_value = meta.Value.Value
                                };

                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    null_value = meta.Value.Value,
                                    index = eMappingIndex.not_analyzed,
                                });
                            }

                            multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                            {
                                name = "analyzed",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = normalSearchAnalyzer,
                                analyzer = normalIndexAnalyzer
                            });
                            multiField.fields.Add(new BasicMappingPropertyV2()
                            {
                                name = "lowercase",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = LOWERCASE_ANALYZER,
                                analyzer = LOWERCASE_ANALYZER
                            });
                            multiField.fields.Add(new BasicMappingPropertyV2()
                            {
                                name = "phrase_autocomplete",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                                analyzer = PHRASE_STARTS_WITH_ANALYZER
                            });

                            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                            {
                                multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = "autocomplete",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = autocompleteSearchAnalyzer,
                                    analyzer = autocompleteIndexAnalyzer
                                });
                            }

                            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
                            {
                                multiField.fields.Add(new BasicMappingPropertyV2()
                                {
                                    name = "phonetic",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = phoneticSearchAnalyzer,
                                    analyzer = phoneticIndexAnalyzer
                                });
                            }

                            metas.AddProperty(multiField);
                        }
                        else
                        {
                            metas.AddProperty(new BasicMappingPropertyV2()
                            {
                                name = sMetaName,
                                type = eESFieldType.DATE,
                                index = eMappingIndex.not_analyzed,
                                format = DATE_FORMAT
                            });
                        }

                        mappedMetas.Add(sMetaName);
                    }
                }
            }

            #endregion

            mappingObj.AddProperty(tags);
            mappingObj.AddProperty(metas);

            return mappingObj.ToString();
        }

        public override string CreateEpgMapping(Dictionary<string, KeyValuePair<eESFieldType, string>> metasMap, List<string> groupTags, MappingAnalyzers specificLanguageAnalyzers,
                                                MappingAnalyzers defaultLanguageAnalyzers, string mappingName, bool shouldAddRouting)
        {
            string normalIndexAnalyzer = specificLanguageAnalyzers.normalIndexAnalyzer;
            string normalSearchAnalyzer = specificLanguageAnalyzers.normalSearchAnalyzer;
            string autocompleteIndexAnalyzer = specificLanguageAnalyzers.autocompleteIndexAnalyzer;
            string autocompleteSearchAnalyzer = specificLanguageAnalyzers.autocompleteSearchAnalyzer;
            string suffix = specificLanguageAnalyzers.suffix;
            string phoneticIndexAnalyzer = specificLanguageAnalyzers.phoneticIndexAnalyzer;
            string phoneticSearchAnalyzer = specificLanguageAnalyzers.phoneticSearchAnalyzer;

            if (metasMap == null || groupTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj(mappingName);

            if (shouldAddRouting)
            {
                ESRouting routing = new ESRouting()
                {
                    path = null,
                    //
                    //  !!! ATTENTION !!!
                    //
                    // routing is not required in this version of ES
                    // This ie because it enforces routing to be specified in EVERY ACTION
                    // INCLUDING DELETE REQUESTS
                    // EVEN IF WE DON'T KNOW THE ROUTING KEY
                    // So in order to avoid this, I don't want the routing to be required
                    // It will be used when inserting and searching
                    // But not when deleting
                    // Alright?
                    //
                    required = false
                };

                mappingObj.SetRouting(routing);
            }

            #region Add basic type mappings - (e.g. epg_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "epg_id",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.LONG
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "group_id",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "epg_channel_id",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "linear_media_id",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "wp_type_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "is_active",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "start_date",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.DATE,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "end_date",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.DATE,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "date_routing",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.STRING
            });

            ElasticSearch.Common.FieldsMappingPropertyV2 nameProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                nameProperty.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = autocompleteSearchAnalyzer,
                    analyzer = autocompleteIndexAnalyzer
                });
            }

            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
            {
                nameProperty.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "phonetic",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = phoneticSearchAnalyzer,
                    analyzer = phoneticIndexAnalyzer
                });
            }

            mappingObj.AddProperty(nameProperty);

            ElasticSearch.Common.FieldsMappingPropertyV2 descrpitionMapping = new ElasticSearch.Common.FieldsMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };

            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
            });
            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            descrpitionMapping.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            descrpitionMapping.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = autocompleteSearchAnalyzer,
                    analyzer = autocompleteIndexAnalyzer
                });
            }

            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
            {
                descrpitionMapping.fields.Add(new BasicMappingPropertyV2()
                {
                    name = "phonetic",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    index = eMappingIndex.analyzed,
                    search_analyzer = phoneticSearchAnalyzer,
                    analyzer = phoneticIndexAnalyzer
                });
            }

            mappingObj.AddProperty(descrpitionMapping);

            ElasticSearch.Common.FieldsMappingPropertyV2 epgIdentifierProperty = new FieldsMappingPropertyV2()
            {
                name = "epg_identifier",
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };

            epgIdentifierProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "epg_identifier",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });

            epgIdentifierProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            epgIdentifierProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            mappingObj.AddProperty(epgIdentifierProperty);

            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "cache_date",
                index = eMappingIndex.not_analyzed,
                type = eESFieldType.DATE,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });

            ElasticSearch.Common.FieldsMappingPropertyV2 cridProperty = new FieldsMappingPropertyV2()
            {
                name = "crid",
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };

            cridProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "crid",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });

            cridProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            cridProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "phrase_autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                analyzer = PHRASE_STARTS_WITH_ANALYZER
            });

            mappingObj.AddProperty(cridProperty);

            #endregion

            #region Add tags mapping
            InnerMappingPropertyV2 tags = new InnerMappingPropertyV2()
            {
                name = "tags"
            };
            foreach (string sTagName in groupTags)
            {
                if (!string.IsNullOrEmpty(sTagName))
                {
                    FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                    {
                        name = AddSuffix(sTagName, suffix),
                        type = eESFieldType.STRING,
                        index = eMappingIndex.analyzed,
                        search_analyzer = LOWERCASE_ANALYZER,
                        analyzer = LOWERCASE_ANALYZER,
                        null_value = ""
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = AddSuffix(sTagName, suffix),
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = string.Empty,
                        index = eMappingIndex.analyzed,
                        search_analyzer = LOWERCASE_ANALYZER,
                        analyzer = LOWERCASE_ANALYZER,
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = "analyzed",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        index = eMappingIndex.analyzed,
                        search_analyzer = normalSearchAnalyzer,
                        analyzer = normalIndexAnalyzer
                    });
                    multiField.fields.Add(new BasicMappingPropertyV2()
                    {
                        name = "lowercase",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        index = eMappingIndex.analyzed,
                        search_analyzer = LOWERCASE_ANALYZER,
                        analyzer = LOWERCASE_ANALYZER
                    });
                    multiField.fields.Add(new BasicMappingPropertyV2()
                    {
                        name = "phrase_autocomplete",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        index = eMappingIndex.analyzed,
                        search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                        analyzer = PHRASE_STARTS_WITH_ANALYZER
                    });

                    if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                    {
                        multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
                        {
                            name = "autocomplete",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            index = eMappingIndex.analyzed,
                            search_analyzer = autocompleteSearchAnalyzer,
                            analyzer = autocompleteIndexAnalyzer
                        });
                    }

                    if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
                    {
                        multiField.fields.Add(new BasicMappingPropertyV2()
                        {
                            name = "phonetic",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            index = eMappingIndex.analyzed,
                            search_analyzer = phoneticSearchAnalyzer,
                            analyzer = phoneticIndexAnalyzer
                        });
                    }

                    tags.AddProperty(multiField);
                }
            }
            #endregion

            #region Add metas mapping
            InnerMappingPropertyV2 metas = new InnerMappingPropertyV2()
            {
                name = "metas"
            };

            if (metasMap != null && metasMap.Count > 0)
            {
                HashSet<string> mappedMetas = new HashSet<string>();
                foreach (KeyValuePair<string, KeyValuePair<eESFieldType, string>> meta in metasMap)
                {
                    string sMetaName = meta.Key;
                    if (!string.IsNullOrEmpty(sMetaName))
                    {
                        sMetaName = sMetaName.ToLower();
                        // Don't create double mapping for metas to avoid errors
                        if (mappedMetas.Contains(sMetaName))
                        {
                            continue;
                        }
                        if (meta.Value.Key != eESFieldType.DATE)
                        {

                            FieldsMappingPropertyV2 multiField = null;

                            if (meta.Value.Key == eESFieldType.STRING)
                            {
                                multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = LOWERCASE_ANALYZER,
                                    analyzer = LOWERCASE_ANALYZER,
                                    null_value = meta.Value.Value
                                };

                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    null_value = meta.Value.Value,
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = LOWERCASE_ANALYZER,
                                    analyzer = LOWERCASE_ANALYZER,
                                });

                                multiField.fields.Add(new BasicMappingPropertyV2()
                                {
                                    name = "phrase_autocomplete",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                                    analyzer = PHRASE_STARTS_WITH_ANALYZER
                                });
                            }
                            else
                            {
                                multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    index = eMappingIndex.not_analyzed,
                                    null_value = meta.Value.Value
                                };

                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = AddSuffix(sMetaName, suffix),
                                    type = meta.Value.Key,
                                    null_value = meta.Value.Value,
                                    index = eMappingIndex.not_analyzed,
                                });
                            }

                            multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                            {
                                name = "analyzed",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = normalSearchAnalyzer,
                                analyzer = normalIndexAnalyzer
                            });
                            multiField.fields.Add(new BasicMappingPropertyV2()
                            {
                                name = "lowercase",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = LOWERCASE_ANALYZER,
                                analyzer = LOWERCASE_ANALYZER
                            });
                            multiField.fields.Add(new BasicMappingPropertyV2()
                            {
                                name = "phrase_autocomplete",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                index = eMappingIndex.analyzed,
                                search_analyzer = PHRASE_STARTS_WITH_SEARCH_ANALYZER,
                                analyzer = PHRASE_STARTS_WITH_ANALYZER
                            });

                            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                            {
                                multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = "autocomplete",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = autocompleteSearchAnalyzer,
                                    analyzer = autocompleteIndexAnalyzer
                                });
                            }

                            if (!string.IsNullOrEmpty(phoneticIndexAnalyzer) && !string.IsNullOrEmpty(phoneticSearchAnalyzer))
                            {
                                multiField.fields.Add(new BasicMappingPropertyV2()
                                {
                                    name = "phonetic",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    index = eMappingIndex.analyzed,
                                    search_analyzer = phoneticSearchAnalyzer,
                                    analyzer = phoneticIndexAnalyzer
                                });
                            }

                            metas.AddProperty(multiField);
                        }
                        else
                        {
                            metas.AddProperty(new BasicMappingPropertyV2()
                            {
                                name = sMetaName,
                                type = eESFieldType.DATE,
                                index = eMappingIndex.not_analyzed,
                                format = DATE_FORMAT
                            });
                        }

                        mappedMetas.Add(sMetaName);
                    }
                }
            }

            #endregion

            mappingObj.AddProperty(metas);
            mappingObj.AddProperty(tags);

            return mappingObj.ToString();
        }

        public override string CreateMetadataMapping(string normalIndexAnalyzer, string normalSearchAnalyzer, string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null, string suffix = null)
        {
            string result = string.Empty;

            ESMappingObj mappingObj = new ESMappingObj(AddSuffix("tag", suffix));

            FieldsMappingPropertyV2 valueProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("value", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            valueProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("value", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            valueProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            valueProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            valueProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = autocompleteSearchAnalyzer,
                analyzer = autocompleteIndexAnalyzer
            });

            mappingObj.AddProperty(valueProperty);
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "tag_id",
                type = eESFieldType.LONG,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "topic_id",
                type = eESFieldType.LONG,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "topic_name",
                type = eESFieldType.STRING,
                index = eMappingIndex.not_analyzed
            });

            result = mappingObj.ToString();

            return result;
        }

        public override string SerializeMetadataObject(long topicId, int tagId, string tagValue)
        {
            string result = string.Empty;
            JObject json = new JObject();

            json["topic_id"] = JToken.FromObject(topicId);
            json["tag_id"] = JToken.FromObject(tagId);
            json["tag_value"] = JToken.FromObject(tagValue);

            result = json.ToString(Newtonsoft.Json.Formatting.None);

            return result;
        }

        public override string SerializeTagValueObject(TagValue tagValue, LanguageObj language)
        {
            string result = string.Empty;
            JObject json = JObject.FromObject(tagValue);

            // if it is not default language, value field should have suffix
            if (!language.IsDefault)
            {
                json.Remove("value");
                string valueField = string.Format("value_{0}", language.Code);
                json[valueField] = tagValue.value;
            }

            result = json.ToString(Newtonsoft.Json.Formatting.None);

            return result;
        }
        public override string CreateChannelMapping(string normalIndexAnalyzer, string normalSearchAnalyzer, string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null, string suffix = null)
        {
            string result = string.Empty;

            ESMappingObj mappingObj = new ESMappingObj(AddSuffix("channel", suffix));

            #region Name
            FieldsMappingPropertyV2 nameProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = autocompleteSearchAnalyzer,
                analyzer = autocompleteIndexAnalyzer
            });

            mappingObj.AddProperty(nameProperty);

            #endregion

            #region SystemName
            FieldsMappingPropertyV2 systemNameProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("system_name", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            systemNameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("system_name", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            systemNameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            systemNameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            systemNameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = autocompleteSearchAnalyzer,
                analyzer = autocompleteIndexAnalyzer
            });

            mappingObj.AddProperty(systemNameProperty);

            #endregion

            #region Description
            FieldsMappingPropertyV2 descriptionProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER,
                null_value = ""
            };
            descriptionProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            descriptionProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = normalSearchAnalyzer,
                analyzer = normalIndexAnalyzer
            });
            descriptionProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "lowercase",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = LOWERCASE_ANALYZER,
                analyzer = LOWERCASE_ANALYZER
            });
            descriptionProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "autocomplete",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = autocompleteSearchAnalyzer,
                analyzer = autocompleteIndexAnalyzer
            });

            mappingObj.AddProperty(descriptionProperty);

            #endregion

            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "channel_type",
                type = eESFieldType.LONG,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "channel_id",
                type = eESFieldType.LONG,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "is_active",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.not_analyzed,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "update_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.not_analyzed,
                format = DATE_FORMAT
            });

            result = mappingObj.ToString();

            return result;
        }

        public override string SerializeChannelObject(Channel channel)
        {
            string result = string.Empty;
            JObject json = new JObject();

            json["name"] = JToken.FromObject(channel.m_sName);
            json["description"] = JToken.FromObject(channel.m_sDescription);
            json["system_name"] = JToken.FromObject(channel.SystemName);
            json["channel_type"] = JToken.FromObject(channel.m_nChannelTypeID);
            json["channel_id"] = JToken.FromObject(channel.m_nChannelID);
            json["is_active"] = JToken.FromObject(channel.m_nIsActive);
            json["create_date"] = JToken.FromObject(channel.CreateDate.Value.ToString("yyyyMMddHHmmss"));
            json["create_date"] = JToken.FromObject(channel.UpdateDate.Value.ToString("yyyyMMddHHmmss"));

            result = json.ToString(Newtonsoft.Json.Formatting.None);

            return result;
        }
    }
}
