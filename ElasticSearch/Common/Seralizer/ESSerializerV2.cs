using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Common
{
    public class ESSerializerV2 : ESSerializerV1
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
            mappingObj.AddProperty(new BasicMappingProperty() { name = "media_id", type = eESFieldType.LONG, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "group_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "media_type_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "wp_type_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "is_active", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "device_rule_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "like_counter", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "start_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "end_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "final_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "create_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "update_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "cache_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "user_types", type = eESFieldType.INTEGER, analyzed = false });

            ElasticSearch.Common.MultiFieldMappingProperty nameProperty = new MultiFieldMappingProperty() { name = "name" };
            nameProperty.fields.Add(new BasicMappingProperty()
            {
                name = "name",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                analyzed = false
            });
            nameProperty.fields.Add(new BasicMappingProperty()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = true,
                search_analyzer = sSearchAnalyzer,
                index_analyzer = sIndexAnalyzer
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                nameProperty.fields.Add(new BasicMappingProperty()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    analyzed = true,
                    search_analyzer = autocompleteSearchAnalyzer,
                    index_analyzer = autocompleteIndexAnalyzer
                });
            }

            mappingObj.AddProperty(nameProperty);

            ElasticSearch.Common.MultiFieldMappingProperty descProperty = new MultiFieldMappingProperty() { name = "description" };

            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
            {
                name = "description",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = false
            });
            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = true,
                search_analyzer = sSearchAnalyzer,
                index_analyzer = sIndexAnalyzer
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                descProperty.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    analyzed = true,
                    search_analyzer = autocompleteSearchAnalyzer,
                    index_analyzer = autocompleteIndexAnalyzer
                });
            }

            mappingObj.AddProperty(descProperty);

            #endregion

            #region Add tags mapping
            InnerMappingProperty tags = new InnerMappingProperty() { name = "tags" };

            if (oGroupTags.Count > 0)
            {
                foreach (int tagID in oGroupTags.Keys)
                {
                    string sTagName = oGroupTags[tagID];

                    if (!string.IsNullOrEmpty(sTagName))
                    {
                        sTagName = sTagName.ToLower();

                        MultiFieldMappingProperty multiField = new ElasticSearch.Common.MultiFieldMappingProperty()
                        {
                            name = sTagName
                        };
                        multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                        {
                            name = sTagName,
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = string.Empty,
                            analyzed = false
                        });
                        multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                        {
                            name = "analyzed",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            analyzed = true,
                            search_analyzer = sSearchAnalyzer,
                            index_analyzer = sIndexAnalyzer
                        });

                        if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                        {
                            multiField.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
                            {
                                name = "autocomplete",
                                type = ElasticSearch.Common.eESFieldType.STRING,
                                null_value = "",
                                analyzed = true,
                                search_analyzer = autocompleteSearchAnalyzer,
                                index_analyzer = autocompleteIndexAnalyzer
                            });
                        }

                        tags.AddProperty(multiField);
                    }
                }
            }

            #endregion

            #region Add metas mapping
            InnerMappingProperty metas = new InnerMappingProperty() { name = "metas" };
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
                                MultiFieldMappingProperty multiField = new ElasticSearch.Common.MultiFieldMappingProperty()
                                {
                                    name = sMetaName
                                };
                                multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                                {
                                    name = sMetaName,
                                    type = eMetaType,
                                    null_value = sNullValue,
                                    analyzed = false
                                });
                                multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                                {
                                    name = "analyzed",
                                    type = ElasticSearch.Common.eESFieldType.STRING,
                                    null_value = "",
                                    analyzed = true,
                                    search_analyzer = sSearchAnalyzer,
                                    index_analyzer = sIndexAnalyzer
                                });

                                if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                                {
                                    multiField.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
                                    {
                                        name = "autocomplete",
                                        type = ElasticSearch.Common.eESFieldType.STRING,
                                        null_value = "",
                                        analyzed = true,
                                        search_analyzer = autocompleteSearchAnalyzer,
                                        index_analyzer = autocompleteIndexAnalyzer
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

        public string CreateEpgMapping(List<string> lMetasNames, List<string> lTags)
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
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "epg_id",
                analyzed = false,
                type = eESFieldType.LONG
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "group_id",
                analyzed = false,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "epg_channel_id",
                analyzed = false,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "is_active",
                analyzed = false,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "start_date",
                analyzed = false,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "end_date",
                analyzed = false,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "date_routing",
                analyzed = false,
                type = eESFieldType.STRING
            });

            ElasticSearch.Common.MultiFieldMappingProperty nameProperty = new MultiFieldMappingProperty()
            {
                name = "name"
            };
            nameProperty.fields.Add(new BasicMappingProperty()
            {
                name = "name",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                analyzed = false
            });
            nameProperty.fields.Add(new BasicMappingProperty()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = true,
                search_analyzer = searchAnalyzer,
                index_analyzer = indexAnalyzer
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                nameProperty.fields.Add(new BasicMappingProperty()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    analyzed = true,
                    search_analyzer = autocompleteSearchAnalyzer,
                    index_analyzer = autocompleteIndexAnalyzer
                });
            }

            mappingObj.AddProperty(nameProperty);

            ElasticSearch.Common.MultiFieldMappingProperty descrpitionMapping = new ElasticSearch.Common.MultiFieldMappingProperty()
            {
                name = "description"
            };

            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
            {
                name = "description",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = false
            });
            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
            {
                name = "analyzed",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = true,
                search_analyzer = searchAnalyzer,
                index_analyzer = indexAnalyzer
            });

            if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
            {
                descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
                {
                    name = "autocomplete",
                    type = ElasticSearch.Common.eESFieldType.STRING,
                    null_value = "",
                    analyzed = true,
                    search_analyzer = autocompleteSearchAnalyzer,
                    index_analyzer = autocompleteIndexAnalyzer
                });
            }

            mappingObj.AddProperty(descrpitionMapping);

            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "cache_date",
                analyzed = false,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingProperty()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                analyzed = false
            });

            #endregion

            #region Add tags mapping
            InnerMappingProperty tags = new InnerMappingProperty()
            {
                name = "tags"
            };
            foreach (string sTagName in lTags)
            {
                if (!string.IsNullOrEmpty(sTagName))
                {
                    MultiFieldMappingProperty multiField = new ElasticSearch.Common.MultiFieldMappingProperty()

                    {
                        name = sTagName
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                    {
                        name = sTagName,
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = string.Empty,
                        analyzed = false
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                    {
                        name = "analyzed",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        analyzed = true,
                        search_analyzer = searchAnalyzer,
                        index_analyzer = indexAnalyzer
                    });

                    if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                    {
                        multiField.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
                        {
                            name = "autocomplete",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            analyzed = true,
                            search_analyzer = autocompleteSearchAnalyzer,
                            index_analyzer = autocompleteIndexAnalyzer
                        });
                    }

                    tags.AddProperty(multiField);
                }
            }
            #endregion

            #region Add metas mapping
            InnerMappingProperty metas = new InnerMappingProperty()
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
                    MultiFieldMappingProperty multiField = new ElasticSearch.Common.MultiFieldMappingProperty()
                    {
                        name = sMetaName
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                    {
                        name = sMetaName,
                        type = eMetaType,
                        null_value = sNullValue,
                        analyzed = false
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingProperty()
                    {
                        name = "analyzed",
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = "",
                        analyzed = true,
                        search_analyzer = searchAnalyzer,
                        index_analyzer = indexAnalyzer
                    });

                    if (!string.IsNullOrEmpty(autocompleteIndexAnalyzer) && !string.IsNullOrEmpty(autocompleteSearchAnalyzer))
                    {
                        multiField.fields.Add(new ElasticSearch.Common.BasicMappingProperty()
                        {
                            name = "autocomplete",
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = "",
                            analyzed = true,
                            search_analyzer = autocompleteSearchAnalyzer,
                            index_analyzer = autocompleteIndexAnalyzer
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

        public string SerializeEpgObject(EpgCB oEpg)
        {
            StringBuilder sRecord = new StringBuilder();
            sRecord.Append("{ ");

            SerializeEPGBody(oEpg, sRecord);

            sRecord.Append(" }");

            return sRecord.ToString();
        }
    }
}
