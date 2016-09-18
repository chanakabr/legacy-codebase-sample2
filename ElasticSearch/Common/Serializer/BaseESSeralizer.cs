using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Common
{
    public abstract class BaseESSeralizer
    {
        protected static readonly string META_DOUBLE_SUFFIX = "_DOUBLE";
        protected static readonly string META_BOOL_SUFFIX = "_BOOL";

        public BaseESSeralizer()
        {
        }

        public virtual string SerializeMediaObject(Media media, string suffix = null)
        {
            StringBuilder recordBuilder = new StringBuilder();
            recordBuilder.Append("{ ");
            recordBuilder.AppendFormat("\"media_id\": {0}, \"group_id\": {1}, \"media_type_id\": {2}, \"wp_type_id\": {3}, \"is_active\": {4}, " +
                "\"device_rule_id\": {5}, \"like_counter\": {6}, \"views\": {7}, \"rating\": {8}, \"votes\": {9}, \"start_date\": \"{10}\", " +
                "\"end_date\": \"{11}\", \"final_date\": \"{12}\", \"create_date\": \"{13}\", \"update_date\": \"{14}\", \"{19}\": \"{15}\", " +
                "\"{20}\": \"{16}\", \"cache_date\": \"{17}\", \"geo_block_rule_id\": {18}, ",
                media.m_nMediaID, media.m_nGroupID, media.m_nMediaTypeID, media.m_nWPTypeID, media.m_nIsActive,
                media.m_nDeviceRuleId, media.m_nLikeCounter, media.m_nViews, media.m_dRating, media.m_nVotes, media.m_sStartDate,
                media.m_sEndDate, media.m_sFinalEndDate, media.m_sCreateDate, media.m_sUpdateDate, Common.Utils.ReplaceDocumentReservedCharacters(ref media.m_sName),
                Common.Utils.ReplaceDocumentReservedCharacters(ref media.m_sDescription), DateTime.UtcNow.ToString("yyyyMMddHHmmss"), media.geoBlockRule,
                // {19}
                AddSuffix("name", suffix),
                // {20}
                AddSuffix("description", suffix)
                );

            #region add media file types

            recordBuilder.Append(" \"media_file_types\": [");
            if (!string.IsNullOrWhiteSpace(media.m_sMFTypes))
            {
                string[] lMFTypesSplited = media.m_sMFTypes.Split(';');
                List<string> lFileTypes = new List<string>();

                for (int i = 0; i < lMFTypesSplited.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lMFTypesSplited[i]))
                        lFileTypes.Add(lMFTypesSplited[i]);
                }


                if (lFileTypes.Count > 0)
                {
                    recordBuilder.Append(
                       lFileTypes.Aggregate((current, next) => current + "," + next)
                       );
                }
            }

            recordBuilder.Append("],");
            #endregion

            #region add user types
            List<string> lUserTypes = new List<string>();
            recordBuilder.Append(" \"user_types\": [");
            if (!string.IsNullOrEmpty(media.m_sUserTypes))
            {
                string[] lUserTypesSplited = media.m_sUserTypes.Split(';');

                foreach (string ut in lUserTypesSplited)
                {
                    if (!string.IsNullOrWhiteSpace(ut))
                        lUserTypes.Add(ut);
                }
                if (lUserTypes.Count > 0)
                {

                }
            }
            else
            {
                lUserTypes.Add("0");
            }
            recordBuilder.Append(string.Join(",", lUserTypes));
            recordBuilder.Append("],");
            #endregion

            #region add metas
            recordBuilder.Append(" \"metas\": {");

            if (media.m_dMeatsValues != null && media.m_dMeatsValues.Count > 0)
            {
                List<string> metaNameValues = new List<string>();
                #region added default lang metas
                foreach (string sMetaName in media.m_dMeatsValues.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(sMetaName))
                    {
                        string sMetaValue = media.m_dMeatsValues[sMetaName];
                        if (!string.IsNullOrWhiteSpace(sMetaValue))
                        {

                            metaNameValues.Add(string.Format(" \"{0}\": \"{1}\"", 
                                AddSuffix(sMetaName.ToLower(), suffix), 
                                Common.Utils.ReplaceDocumentReservedCharacters(ref sMetaValue)));
                        }
                    }
                }

                if (metaNameValues.Count > 0)
                    recordBuilder.Append(string.Join(",", metaNameValues));
                #endregion

            }
            recordBuilder.Append("},");
            #endregion

            #region add tags
            recordBuilder.Append(" \"tags\": {");

            if (media.m_dTagValues != null && media.m_dTagValues.Count > 0)
            {
                List<string> tagNameValues = new List<string>();

                foreach (string sTagName in media.m_dTagValues.Keys)
                {
                    if (string.IsNullOrEmpty(sTagName))
                        continue;

                    List<string> lTagValues = new List<string>();
                    foreach (var tagID in media.m_dTagValues[sTagName].Keys)
                    {
                        string sTagVal = media.m_dTagValues[sTagName][tagID];
                        string sEscapedTagVal = Common.Utils.ReplaceDocumentReservedCharacters(ref sTagVal);

                        if (!string.IsNullOrEmpty(sEscapedTagVal))
                            lTagValues.Add(string.Concat("\"", sEscapedTagVal, "\""));

                    }
                    if (lTagValues.Count > 0)
                    {
                        string sJoinedTagVals = string.Join(",", lTagValues);
                        tagNameValues.Add(string.Format(" \"{0}\": [ {1} ]",
                            AddSuffix(sTagName.ToLower(), suffix), 
                            sJoinedTagVals));
                    }
                }
                if (tagNameValues.Count > 0)
                    recordBuilder.Append(string.Join(",", tagNameValues));
            }

            recordBuilder.Append(" }");

            #endregion

            #region add regions

            // Add this field only if there are regions on the media object
            if (media.regions != null && media.regions.Count > 0)
            {
                recordBuilder.Append(", \"regions\": [");

                foreach (int regionId in media.regions)
                {
                    recordBuilder.Append(regionId);
                    recordBuilder.Append(',');
                }

                // Remove last ','
                recordBuilder.Remove(recordBuilder.Length - 1, 1);

                recordBuilder.Append("]");
            }

            #endregion

            #region Add Is Free + Free file types

            // Add this field only if there are free file types on the media object
            if (media.freeFileTypes != null && media.freeFileTypes.Count > 0)
            {
                recordBuilder.Append(", \"free_file_types\": [");

                foreach (int fileTypeId in media.freeFileTypes)
                {
                    recordBuilder.Append(fileTypeId);
                    recordBuilder.Append(',');
                }

                // Remove last ','
                recordBuilder.Remove(recordBuilder.Length - 1, 1);

                recordBuilder.Append("]");
            }

            recordBuilder.Append(", \"is_free\": ");
            recordBuilder.Append(Convert.ToInt32(media.isFree));

            #endregion

            #region EPG Identifier

            // Add this field only if it has a value
            if (!string.IsNullOrEmpty(media.epgIdentifier))
            {
                recordBuilder.Append(", \"epg_identifier\": \"");

                recordBuilder.Append(media.epgIdentifier);

                recordBuilder.Append("\"");
            }
            #endregion

            recordBuilder.Append(" }");

            return recordBuilder.ToString();

        }

        public virtual string CreateMediaMapping(Dictionary<int, Dictionary<string, string>> oMetasValuesByGroupId, Dictionary<int, string> oGroupTags,
            string sIndexAnalyzer, string sSearchAnalyzer, string autocompleteIndexAnalyzer = null, string autocompleteSearchAnalyzer = null, string suffix = null)
        {
            if (oMetasValuesByGroupId == null || oGroupTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj("media");

            #region Add basic type mappings - (e.g. media_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "media_id",
                type = eESFieldType.LONG,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "group_id",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "media_type_id",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "wp_type_id",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "is_active",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "device_rule_id",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "like_counter",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "views",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "rating",
                type = eESFieldType.DOUBLE,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "votes",
                type = eESFieldType.INTEGER,
                analyzed = false,
                null_value = "0"
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "start_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "end_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "final_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "update_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "cache_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "user_types",
                type = eESFieldType.INTEGER,
                analyzed = false
            });

            ElasticSearch.Common.MultiFieldMappingPropertyV1 nameProperty = new MultiFieldMappingPropertyV1()
            {
                name = "name"
            };
            nameProperty.fields.Add(new BasicMappingPropertyV1()
            {
                name = "name",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                analyzed = false
            });
            nameProperty.fields.Add(new BasicMappingPropertyV1()
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
                nameProperty.fields.Add(new BasicMappingPropertyV1()
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

            ElasticSearch.Common.MultiFieldMappingPropertyV1 descProperty = new MultiFieldMappingPropertyV1()
            {
                name = "description"
            };

            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
            {
                name = "description",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = false
            });
            descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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
                descProperty.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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
            InnerMappingPropertyV1 tags = new InnerMappingPropertyV1()
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

                        MultiFieldMappingPropertyV1 multiField = new ElasticSearch.Common.MultiFieldMappingPropertyV1()
                        {
                            name = sTagName
                        };
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
                        {
                            name = sTagName,
                            type = ElasticSearch.Common.eESFieldType.STRING,
                            null_value = string.Empty,
                            analyzed = false
                        });
                        multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
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
                            multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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
            InnerMappingPropertyV1 metas = new InnerMappingPropertyV1()
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
                                MultiFieldMappingPropertyV1 multiField = new ElasticSearch.Common.MultiFieldMappingPropertyV1()
                                {
                                    name = sMetaName
                                };
                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
                                {
                                    name = sMetaName,
                                    type = eMetaType,
                                    null_value = sNullValue,
                                    analyzed = false
                                });
                                multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
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
                                    multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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

        protected virtual void GetMetaType(string sMeta, out eESFieldType sMetaType, out string sNullValue)
        {
            sMetaType = eESFieldType.STRING;
            sNullValue = string.Empty;

            if (sMeta.Contains(META_BOOL_SUFFIX))
            {
                sMetaType = eESFieldType.INTEGER;
                sNullValue = "0";
            }
            else if (sMeta.Contains(META_DOUBLE_SUFFIX))
            {
                sMetaType = eESFieldType.DOUBLE;
                sNullValue = "0.0";
            }
        }

        public virtual string CreateEpgMapping(List<string> lMetasNames, List<string> lTags, string mappingName)
        {
            return CreateEpgMapping(lMetasNames, lTags, string.Empty, string.Empty, mappingName);
        }

        public virtual string CreateEpgMapping(List<string> lMetasNames, List<string> lTags, string indexAnalyzer, string searchAnalyzer,
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
                    path = "date_routing",
                    required = true
                };
                mappingObj.SetRouting(routing);
            }

            #region Add basic type mappings - (e.g. epg_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "epg_id",
                analyzed = false,
                type = eESFieldType.LONG
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "group_id",
                analyzed = false,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "epg_channel_id",
                analyzed = false,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "is_active",
                analyzed = false,
                type = eESFieldType.INTEGER
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "start_date",
                analyzed = false,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "end_date",
                analyzed = false,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "date_routing",
                analyzed = false,
                type = eESFieldType.STRING
            });

            ElasticSearch.Common.MultiFieldMappingPropertyV1 nameProperty = new MultiFieldMappingPropertyV1()
            {
                name = "name"
            };
            nameProperty.fields.Add(new BasicMappingPropertyV1()
            {
                name = "name",
                type = eESFieldType.STRING,
                null_value = string.Empty,
                analyzed = false
            });
            nameProperty.fields.Add(new BasicMappingPropertyV1()
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
                nameProperty.fields.Add(new BasicMappingPropertyV1()
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

            ElasticSearch.Common.MultiFieldMappingPropertyV1 descrpitionMapping = new ElasticSearch.Common.MultiFieldMappingPropertyV1()
            {
                name = "description"
            };

            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
            {
                name = "description",
                type = ElasticSearch.Common.eESFieldType.STRING,
                null_value = "",
                analyzed = false
            });
            descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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
                descrpitionMapping.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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

            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "cache_date",
                analyzed = false,
                type = eESFieldType.DATE
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "create_date",
                type = eESFieldType.DATE,
                analyzed = false
            });
            mappingObj.AddProperty(new BasicMappingPropertyV1()
            {
                name = "crid",
                analyzed = false,
                null_value = "",
                type = eESFieldType.STRING
            });

            #endregion

            #region Add tags mapping
            InnerMappingPropertyV1 tags = new InnerMappingPropertyV1()
            {
                name = "tags"
            };
            foreach (string sTagName in lTags)
            {
                if (!string.IsNullOrEmpty(sTagName))
                {
                    MultiFieldMappingPropertyV1 multiField = new ElasticSearch.Common.MultiFieldMappingPropertyV1()

                    {
                        name = sTagName
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
                    {
                        name = sTagName,
                        type = ElasticSearch.Common.eESFieldType.STRING,
                        null_value = string.Empty,
                        analyzed = false
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
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
                        multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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
            InnerMappingPropertyV1 metas = new InnerMappingPropertyV1()
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
                    MultiFieldMappingPropertyV1 multiField = new ElasticSearch.Common.MultiFieldMappingPropertyV1()
                    {
                        name = sMetaName
                    };
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
                    {
                        name = sMetaName,
                        type = eMetaType,
                        null_value = sNullValue,
                        analyzed = false
                    });
                    multiField.AddField(new ElasticSearch.Common.BasicMappingPropertyV1()
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
                        multiField.fields.Add(new ElasticSearch.Common.BasicMappingPropertyV1()
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

        public virtual string SerializeEpgObject(EpgCB oEpg, string suffix = null)
        {
            StringBuilder sRecord = new StringBuilder();
            sRecord.Append("{ ");

            SerializeEPGBody(oEpg, sRecord, suffix, true);

            sRecord.Append(" }");

            return sRecord.ToString();
        }

        protected virtual void SerializeEPGBody(EpgCB oEpg, StringBuilder sRecord, string suffix = null, bool withRouting = true)
        {
            string name = oEpg.Name;
            string description = oEpg.Description;

            sRecord.AppendFormat("\"epg_id\": {0}, \"group_id\": {1}, \"epg_channel_id\": {2}, \"is_active\": {3}, \"start_date\": \"{4}\", \"end_date\": \"{5}\"," +
                " \"{13}\": \"{6}\", \"{14}\": \"{7}\", \"cache_date\": \"{8}\", \"create_date\": \"{9}\", \"update_date\": \"{10}\"," +
                "\"search_end_date\": \"{11}\", \"crid\": \"{12}\",",
                oEpg.EpgID, oEpg.GroupID, oEpg.ChannelID, (oEpg.isActive) ? 1 : 0, oEpg.StartDate.ToString("yyyyMMddHHmmss"), oEpg.EndDate.ToString("yyyyMMddHHmmss"),
                Common.Utils.ReplaceDocumentReservedCharacters(ref name), Common.Utils.ReplaceDocumentReservedCharacters(ref description),
                /* cache_date*/ DateTime.UtcNow.ToString("yyyyMMddHHmmss"), 
                oEpg.CreateDate.ToString("yyyyMMddHHmmss"),
                oEpg.UpdateDate.ToString("yyyyMMddHHmmss"),
                oEpg.SearchEndDate.ToString("yyyyMMddHHmmss"),
                oEpg.Crid,
                // {13}
                AddSuffix("name", suffix),
                // {14}
                AddSuffix("description", suffix)
                );

            if (withRouting)
            {
                sRecord.AppendFormat("\"date_routing\" : {0},", oEpg.StartDate.ToUniversalTime().ToString("yyyyMMdd"));
            }

            #region add metas
            sRecord.Append(" \"metas\": {");

            if (oEpg.Metas != null && oEpg.Metas.Keys.Count > 0)
            {
                List<string> metaNameValues = new List<string>();
                string sTrimed;
                foreach (string sMetaName in oEpg.Metas.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(sMetaName))
                    {
                        List<string> lMetaValues = oEpg.Metas[sMetaName];
                        if (lMetaValues != null && lMetaValues.Count > 0)
                        {
                            for (int i = 0; i < lMetaValues.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(lMetaValues[i]))
                                {
                                    sTrimed = lMetaValues[i].Trim();
                                    lMetaValues[i] = string.Format("\"{0}\"", Common.Utils.ReplaceDocumentReservedCharacters(ref sTrimed));
                                }
                            }

                            metaNameValues.Add(string.Format(" \"{0}\": [ {1} ]", 
                                AddSuffix(sMetaName.ToLower(), suffix), 
                                lMetaValues.Aggregate((current, next) => current + "," + next)));
                        }
                    }
                }

                if (metaNameValues.Count > 0)
                    sRecord.Append(metaNameValues.Aggregate((current, next) => current + "," + next));
            }
            sRecord.Append("},");
            #endregion

            #region add tags
            sRecord.Append(" \"tags\": {");

            if (oEpg.Tags != null && oEpg.Tags.Keys.Count > 0)
            {
                List<string> tagNameValues = new List<string>();
                string sTrimed;
                foreach (string sTagName in oEpg.Tags.Keys)
                {
                    if (!string.IsNullOrEmpty(sTagName))
                    {
                        List<string> lTagValues = oEpg.Tags[sTagName];
                        if (lTagValues != null && lTagValues.Count > 0)
                        {
                            for (int i = 0; i < lTagValues.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(lTagValues[i]))
                                {
                                    sTrimed = lTagValues[i].Trim();
                                    lTagValues[i] = string.Format("\"{0}\"", Common.Utils.ReplaceDocumentReservedCharacters(ref sTrimed));
                                }
                            }

                            tagNameValues.Add(string.Format(" \"{0}\": [ {1} ]",
                                AddSuffix(sTagName.ToLower(), suffix), 
                                lTagValues.Aggregate((current, next) => current + "," + next)));
                        }
                    }
                }
                if (tagNameValues.Count > 0)
                    sRecord.Append(tagNameValues.Aggregate((current, next) => current + "," + next));
            }

            sRecord.Append(" }");

            #endregion

        }

        public virtual string SerializeRecordingObject(EpgCB oEpg, long recordingId, string suffix = null)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{ ");
            builder.AppendFormat("\"recording_id\": {0},", recordingId);

            SerializeEPGBody(oEpg, builder, suffix, false);

            builder.Append(" }");

            return builder.ToString();
        }

        protected static string AddSuffix(string text, string suffix)
        {
            string result = text;

            if (!string.IsNullOrEmpty(suffix))
            {
                result = string.Format("{0}_{1}", result, suffix);
            }

            return result;
        }
    }
}
