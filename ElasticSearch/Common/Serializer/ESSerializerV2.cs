using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ESSerializerV2()
        {
        }

        /// <summary>
        /// Read things like this:
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/breaking_20_mapping_changes.html
        /// and this:
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping.html
        /// </summary>
        /// <param name="oMetasValuesByGroupId"></param>
        /// <param name="oGroupTags"></param>
        /// <param name="sIndexAnalyzer"></param>
        /// <param name="sSearchAnalyzer"></param>
        /// <param name="autocompleteIndexAnalyzer"></param>
        /// <param name="autocompleteSearchAnalyzer"></param>
        /// <returns></returns>
        public override string CreateMediaMapping(Dictionary<int, Dictionary<string, string>> oMetasValuesByGroupId, Dictionary<int, string> oGroupTags,
            string sIndexAnalyzer, string sSearchAnalyzer, string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null, string suffix = null)
        {
            if (oMetasValuesByGroupId == null || oGroupTags == null)
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
                index = eMappingIndex.not_analyzed,
                null_value = ""
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.not_analyzed
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = sSearchAnalyzer,
                analyzer = sIndexAnalyzer
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

            mappingObj.AddProperty(nameProperty);

            ElasticSearch.Common.FieldsMappingPropertyV2 descProperty = new FieldsMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.not_analyzed,
                null_value = ""
            };

            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.not_analyzed
            });
            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = sSearchAnalyzer,
                analyzer = sIndexAnalyzer
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

            mappingObj.AddProperty(descProperty);

            #endregion

            #region Add tags mapping
            InnerMappingPropertyV2 tags = new InnerMappingPropertyV2()
            {
                name = "tags"
            };

            if (oGroupTags.Count > 0)
            {
                foreach (int tagID in oGroupTags.Keys)
                {
                    string sTagName = oGroupTags[tagID];

                    if (!string.IsNullOrEmpty(sTagName))
                    {
                        sTagName = sTagName.ToLower();

                        FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                        {
                            name = AddSuffix(sTagName, suffix),
                            type = eESFieldType.STRING,
                            index = eMappingIndex.not_analyzed,
                            null_value = ""
                        };
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                        {
                            name = AddSuffix(sTagName, suffix),
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = string.Empty,
                            index = eMappingIndex.not_analyzed
                        });
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                        {
                            name = "analyzed",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            index = eMappingIndex.analyzed,
                            search_analyzer = sSearchAnalyzer,
                            analyzer = sIndexAnalyzer
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

                        tags.AddProperty(multiField);
                    }
                }
            }

            #endregion

            #region Add metas mapping

            InnerMappingPropertyV2 metas = new InnerMappingPropertyV2()
            {
                name = "metas"
            };

            if (oMetasValuesByGroupId != null)
            {
                foreach (int groupID in oMetasValuesByGroupId.Keys)
                {
                    Dictionary<string, string> dMetas = oMetasValuesByGroupId[groupID];
                    if (dMetas != null)
                    {
                        foreach (string sMeta in dMetas.Keys)
                        {
                            string sMetaName = dMetas[sMeta];

                            if (!string.IsNullOrEmpty(sMetaName))
                            {
                                sMetaName = sMetaName.ToLower();
                                string sNullValue;
                                eESFieldType eMetaType;

                                GetMetaType(sMeta, out eMetaType, out sNullValue);

                                if (eMetaType != eESFieldType.DATE)
                                {
                                    FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                                    {
                                        name = AddSuffix(sMetaName, suffix),
                                        type = eMetaType,
                                        index = eMappingIndex.not_analyzed,
                                        null_value = sNullValue
                                    };
                                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                    {
                                        name = AddSuffix(sMetaName, suffix),
                                        type = eMetaType,
                                        null_value = sNullValue,
                                        index = eMappingIndex.not_analyzed
                                    });
                                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                    {
                                        name = "analyzed",
                                        type = ElasticSearch.Common.eESFieldType.STRING,
                                        null_value = "",
                                        index = eMappingIndex.analyzed,
                                        search_analyzer = sSearchAnalyzer,
                                        analyzer = sIndexAnalyzer
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
                                    metas.AddProperty(multiField);
                                }
                                else
                                {
                                    mappingObj.AddProperty(new BasicMappingPropertyV2()
                                    {
                                        name = sMetaName,
                                        type = eESFieldType.DATE,
                                        index = eMappingIndex.not_analyzed,
                                        format = DATE_FORMAT
                                    });
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            mappingObj.AddProperty(tags);
            mappingObj.AddProperty(metas);

            return mappingObj.ToString();
        }

        public override string CreateEpgMapping(List<string> lMetasNames, List<string> lTags, string mappingName)
        {
            return CreateEpgMapping(lMetasNames, lTags, string.Empty, string.Empty, mappingName);
        }

        public override string CreateEpgMapping(List<string> lMetasNames, List<string> lTags, string indexAnalyzer, string searchAnalyzer,
                                                string mappingName, string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null,
                                                string suffix = null,
                                                bool shouldAddRouting = true)
        {
            if (lMetasNames == null || lTags == null)
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
                index = eMappingIndex.not_analyzed,
                null_value = ""
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = AddSuffix("name", suffix),
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.not_analyzed
            });
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = searchAnalyzer,
                analyzer = indexAnalyzer
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

            mappingObj.AddProperty(nameProperty);

            ElasticSearch.Common.FieldsMappingPropertyV2 descrpitionMapping = new ElasticSearch.Common.FieldsMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = eESFieldType.STRING,
                index = eMappingIndex.not_analyzed,
                null_value = ""
            };

            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = AddSuffix("description", suffix),
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.not_analyzed
            });
            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.analyzed,
                search_analyzer = searchAnalyzer,
                analyzer = indexAnalyzer
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

            mappingObj.AddProperty(descrpitionMapping);

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
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "crid",
                index = eMappingIndex.not_analyzed,
                null_value = "",
                type = eESFieldType.STRING
            });
            #endregion

            #region Add tags mapping
            InnerMappingPropertyV2 tags = new InnerMappingPropertyV2()
            {
                name = "tags"
            };
            foreach (string sTagName in lTags)
            {
                if (!string.IsNullOrEmpty(sTagName))
                {
                    FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                    {
                        name = AddSuffix(sTagName, suffix),
                        type = eESFieldType.STRING,
                        index = eMappingIndex.not_analyzed,
                        null_value = ""
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = AddSuffix(sTagName, suffix),
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = string.Empty,
                        index = eMappingIndex.not_analyzed
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = "analyzed",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        index = eMappingIndex.analyzed,
                        search_analyzer = searchAnalyzer,
                        analyzer = indexAnalyzer
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

                    tags.AddProperty(multiField);
                }
            }
            #endregion

            #region Add metas mapping
            InnerMappingPropertyV2 metas = new InnerMappingPropertyV2()
            {
                name = "metas"
            };

            foreach (string metaName in lMetasNames)
            {
                if (!string.IsNullOrEmpty(metaName))
                {
                    string sMetaName = metaName.ToLower();
                    string sNullValue;
                    eESFieldType eMetaType;
                    GetMetaType(metaName, out eMetaType, out sNullValue);
                    FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                    {
                        name = AddSuffix(sMetaName, suffix),
                        type = eMetaType,
                        index = eMappingIndex.not_analyzed,
                        null_value = sNullValue
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = AddSuffix(sMetaName, suffix),
                        type = eMetaType,
                        null_value = sNullValue,
                        index = eMappingIndex.not_analyzed
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = "analyzed",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        index = eMappingIndex.analyzed,
                        search_analyzer = searchAnalyzer,
                        analyzer = indexAnalyzer
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

                    metas.AddProperty(multiField);

                }
            }

            #endregion

            mappingObj.AddProperty(metas);
            mappingObj.AddProperty(tags);

            return mappingObj.ToString();
        }
    }
}
