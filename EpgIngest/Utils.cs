using ApiObjects;
using ApiObjects.Epg;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using KLogMonitor;
using System.Reflection;

namespace EpgIngest
{
    internal static class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static List<LanguageObj> GetLanguages(int nGroupID)
        {
            List<LanguageObj> lLang = new List<LanguageObj>();
            try
            {
                lLang = CatalogDAL.GetGroupLanguages(nGroupID);
                return lLang;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new List<LanguageObj>();
            }
        }

        internal static List<FieldTypeEntity> GetMappingFields(int nGroupID)
        {
            try
            {
                List<FieldTypeEntity> AllFieldTypeMapping = new List<FieldTypeEntity>();
                GroupManager groupManager = new GroupManager();
                List<int> lSubTree = groupManager.GetSubGroup(nGroupID);

                DataSet ds = EpgDal.GetEpgMappingFields(lSubTree, nGroupID);

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 5)
                {
                    if (ds.Tables[0] != null)//basic
                    {
                        InitializeMappingFields(ds.Tables[0], ds.Tables[3], null,FieldTypes.Basic, ref AllFieldTypeMapping);
                    }
                    if (ds.Tables[1] != null)//metas
                    {
                        InitializeMappingFields(ds.Tables[1], ds.Tables[3], ds.Tables[5] ,FieldTypes.Meta, ref AllFieldTypeMapping);
                    }
                    if (ds.Tables[2] != null)//Tags
                    {
                        InitializeMappingFields(ds.Tables[2], ds.Tables[3], ds.Tables[5], FieldTypes.Tag, ref AllFieldTypeMapping);
                    }

                }

                return AllFieldTypeMapping;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new List<FieldTypeEntity>();
            }
        }

        internal static bool ParseEPGStrToDate(string dateStr, ref DateTime theDate)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14)
                return false;

            string format = "yyyyMMddHHmmss";
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        //Insert rows of table to the db at once using bulk operation.      
        internal static void InsertBulk(DataTable dt, string sTableName, string sConnName)
        {
            if (dt != null)
            {
                ODBCWrapper.InsertQuery insertMessagesBulk = new ODBCWrapper.InsertQuery();
                insertMessagesBulk.SetConnectionKey(sConnName);
                try
                {
                    insertMessagesBulk.InsertBulk(sTableName, dt);
                }
                catch (Exception ex)
                {
                    #region Logging
                    log.Error("", ex);

                    #endregion
                }
                finally
                {
                    if (insertMessagesBulk != null)
                    {
                        insertMessagesBulk.Finish();
                    }
                    insertMessagesBulk = null;
                }
            }
        }

        internal static void GenerateTagsAndValues(ApiObjects.EpgCB epg, List<FieldTypeEntity> FieldEntityMapping, ref Dictionary<int, List<string>> tagsAndValues)
        {
            foreach (string tagType in epg.Tags.Keys)
            {
                string tagTypel = tagType.ToLower();
                int tagTypeID = 0;
                List<FieldTypeEntity> tagField = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Tag && x.Name.ToLower() == tagTypel).ToList();
                if (tagField != null && tagField.Count > 0)
                {
                    tagTypeID = tagField[0].ID;
                }
                else
                {
                    log.Debug("UpdateExistingTagValuesPerEPG - " + string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", tagType, epg.EpgID));
                    continue;//missing tag definition in DB (in FieldEntityMapping)                        
                }

                if (!tagsAndValues.ContainsKey(tagTypeID))
                {
                    tagsAndValues.Add(tagTypeID, new List<string>());
                }
                foreach (string tagValue in epg.Tags[tagType])
                {
                    if (!tagsAndValues[tagTypeID].Contains(tagValue.ToLower()))
                        tagsAndValues[tagTypeID].Add(tagValue.ToLower());
                }
            }
        }

        //Build docids with languages per programid 
        internal static List<string> BuildDocIdsToRemoveGroupPrograms(List<int> lProgramsID, List<LanguageObj> lLanguage)
        {
            List<string> docIds = new List<string>();
            string docID = string.Empty;
            //build key for languages by languageListObj

            foreach (int id in lProgramsID)
            {
                foreach (LanguageObj language in lLanguage)
                {
                    if (language.IsDefault)// main language
                    {
                        docID = id.ToString();
                    }
                    else
                    {
                        docID = string.Format("epg_{0}_lang_{1}", id, language.Code.ToLower());
                    }
                    docIds.Add(docID);
                }
            }

            return docIds;
        }


        // initialize each item with all external_ref  
        private static void InitializeMappingFields(DataTable dataTable, DataTable dataTableRef, DataTable dataTableAlias, FieldTypes fieldTypes, ref List<FieldTypeEntity> AllFieldTypeMapping)
        {
            
            foreach (DataRow dr in dataTable.Rows)
            {
                FieldTypeEntity item = new FieldTypeEntity();
                item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
                item.FieldType = fieldTypes;

                if (fieldTypes != FieldTypes.Basic)
                {
                    DataRow drAlias = dataTableAlias.Select("type = " + (int)fieldTypes + " and field_id = " + item.ID).FirstOrDefault();
                    item.Alias = ODBCWrapper.Utils.GetSafeStr(drAlias, "name");
                    item.RegexExpression = ODBCWrapper.Utils.GetSafeStr(drAlias, "regex_expression");

                    foreach (var x in dataTableRef.Select("type = " + (int)fieldTypes + " and field_id = " + item.ID))
                    {

                        if (item.XmlReffName == null)
                        {
                            item.XmlReffName = new List<string>();
                            item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                        }
                        else
                        {
                            item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                        }
                    }
                }

                AllFieldTypeMapping.Add(item);
            }
        }
    }
}
