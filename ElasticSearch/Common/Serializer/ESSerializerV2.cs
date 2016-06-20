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
        public ESSerializerV2()
        {
        }

        public override string CreateMediaMapping(Dictionary<int, Dictionary<string, string>> oMetasValuesByGroupId, Dictionary<int, string> oGroupTags,
            string sIndexAnalyzer, string sSearchAnalyzer, string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null)
        {
            if (oMetasValuesByGroupId == null || oGroupTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj("media");

            #region Add basic type mappings - (e.g. media_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "media_id",
                type = eESFieldType.LONG,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "group_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "media_type_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "wp_type_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "is_active",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "device_rule_id",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "like_counter",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "start_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no,
                format = "dateOptionalTime"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "end_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no,
                format = "dateOptionalTime"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "final_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no,
                format = "dateOptionalTime"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no,
                format = "dateOptionalTime"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "update_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "cache_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no,
                format = "dateOptionalTime"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "user_types",
                type = eESFieldType.INTEGER,
                index = eMappingIndex.no
            });

            ElasticSearch.Common.FieldsMappingPropertyV2 nameProperty = new FieldsMappingPropertyV2()
            {
                name = "name"
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "name",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.no
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
                name = "description"
            };

            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = "description",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.no
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
                            name = sTagName
                        };
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                        {
                            name = sTagName,
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = string.Empty,
                            index = eMappingIndex.no
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
                                FieldsMappingPropertyV2 multiField = new ElasticSearch.Common.FieldsMappingPropertyV2()
                                {
                                    name = sMetaName
                                };
                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                                {
                                    name = sMetaName,
                                    type = eMetaType,
                                    null_value = sNullValue,
                                    index = eMappingIndex.no
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
                        }
                    }
                }
            }

            #endregion

            mappingObj.AddProperty(tags);
            mappingObj.AddProperty(metas);

            return mappingObj.ToString();
        }

        public override string CreateEpgMapping(List<string> lMetasNames, List<string> lTags)
        {
            return CreateEpgMapping(lMetasNames, lTags, string.Empty, string.Empty);
        }

        public override string CreateEpgMapping(List<string> lMetasNames, List<string> lTags, string indexAnalyzer, string searchAnalyzer,
            string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null)
        {
            if (lMetasNames == null || lTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj("epg");

            ESRouting routing = new ESRouting()
            {
                path = "date_routing",
                required = true
            };
            mappingObj.SetRoting(routing);

            #region Add basic type mappings - (e.g. epg_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "epg_id",
                index = eMappingIndex.no,
                type = eESFieldType.LONG
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "group_id",
                index = eMappingIndex.no,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "epg_channel_id",
                index = eMappingIndex.no,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "is_active",
                index = eMappingIndex.no,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "start_date",
                index = eMappingIndex.no,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "end_date",
                index = eMappingIndex.no,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "date_routing",
                index = eMappingIndex.no,
                type = eESFieldType.STRING
            });

            ElasticSearch.Common.FieldsMappingPropertyV2 nameProperty = new FieldsMappingPropertyV2()
            {
                name = "name"
            };
            nameProperty.fields.Add(new BasicMappingPropertyV2()
            {
                name = "name",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                index = eMappingIndex.no
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
                name = "description"
            };

            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV2()
            {
                name = "description",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                index = eMappingIndex.no
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
                index = eMappingIndex.no,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingPropertyV2()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                index = eMappingIndex.no
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
                        name = sTagName
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = sTagName,
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = string.Empty,
                        index = eMappingIndex.no
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
                        name = sMetaName
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV2()
                    {
                        name = sMetaName,
                        type = eMetaType,
                        null_value = sNullValue,
                        index = eMappingIndex.no
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
