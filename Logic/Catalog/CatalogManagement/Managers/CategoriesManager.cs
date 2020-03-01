using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class CategoriesManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static Tuple<Dictionary<long, CategoryParentCache>, bool> BuildGroupCategories(Dictionary<string, object> funcParams)
        {
            Dictionary<long, CategoryParentCache> result = new Dictionary<long, CategoryParentCache>();
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        result = GetCategoriesIds(groupId.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"BuildGroupCategories failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<Dictionary<long, CategoryParentCache>, bool>(result, success);
        }

        private static Dictionary<long, CategoryParentCache> GetCategoriesIds(int groupId)
        {
            Dictionary<long, CategoryParentCache> categoryItems = new Dictionary<long, CategoryParentCache>();

            try
            {
                DataTable dt = CatalogDAL.GetCategories(groupId);
                if (dt?.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                        CategoryParentCache categoryParentCache = new CategoryParentCache()
                        {
                            ParentId = ODBCWrapper.Utils.GetLongSafeVal(dr, "PARENT_CATEGORY_ID"),
                            Order = ODBCWrapper.Utils.GetIntSafeVal(dr, "ORDER_NUM")
                        };

                        categoryItems.Add(id, categoryParentCache);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while getting categories. group id = {groupId}", ex);
            }

            return categoryItems;
        }

        private static Tuple<CategoryItem, bool> BuildCategoryItem(Dictionary<string, object> funcParams)
        {
            CategoryItem result = null;
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("id"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    long? id = funcParams["id"] as long?;
                    if (groupId.HasValue)
                    {
                        result = GetCategory(groupId.Value, id.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"BuildCategoryItem failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<CategoryItem, bool>(result, success);
        }

        private static CategoryItem GetCategory(int groupId, long id)
        {
            CategoryItem categoryItem = null;

            try
            {
                DataSet ds = CatalogDAL.GetCategoryItem(groupId, id);
                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    categoryItem = new CategoryItem()
                    {
                        Id = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "ID"),
                        ParentId = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "PARENT_CATEGORY_ID"),
                        Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "CATEGORY_NAME"),
                    };

                    bool hasDynamicData = ODBCWrapper.Utils.ExtractBoolean(ds.Tables[0].Rows[0], "HAS_METADATA");
                    if (hasDynamicData)
                    {
                        categoryItem.DynamicData = CatalogDAL.GetCategoryDynamicData(id);
                    }

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        categoryItem.UnifiedChannels = new List<UnifiedChannel>();

                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            categoryItem.UnifiedChannels.Add(new UnifiedChannel()
                            {
                                Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "CHANNEL_ID"),
                                Type = (UnifiedChannelType)ODBCWrapper.Utils.GetLongSafeVal(dr, "CHANNEL_TYPE")
                            });

                            //TODO anat: check if channel exist 
                        }
                    }

                    if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                    {
                        categoryItem.NamesInOtherLanguages = new List<LanguageContainer>();

                        CatalogGroupCache catalogGroupCache = null;
                        if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                        {
                            log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling HandleNamesInOtherLanguages");
                            return null;
                        }

                        foreach (DataRow dr in ds.Tables[2].Rows)
                        {
                            categoryItem.NamesInOtherLanguages.Add(new LanguageContainer()
                            {
                                m_sValue = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                                m_sLanguageCode3 = catalogGroupCache.LanguageMapById[ODBCWrapper.Utils.GetIntSafeVal(dr, "LANGUAGE_ID")].Code
                            });
                        }
                    }

                    // Set ChildCategoriesIds
                    var groupCategoriesIds = GetGroupCategoriesIds(groupId);
                    if (groupCategoriesIds != null)
                    {
                        categoryItem.ChildrenIds = groupCategoriesIds.Where(x => x.Value.ParentId == id).OrderBy(y => y.Value.Order).Select(z => z.Key).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while getting GetCategoryItembyDb. group id = {groupId}", ex);
            }

            return categoryItem;
        }

        internal static Dictionary<long, CategoryParentCache> GetGroupCategoriesIds(int groupId, List<long> ids = null, bool rootOnly = false)
        {
            // save mapping between categoryItem and Parentcategory
            Dictionary<long, CategoryParentCache> result = new Dictionary<long, CategoryParentCache>();

            try
            {
                Dictionary<long, CategoryParentCache> groupCategoriesIds = new Dictionary<long, CategoryParentCache>();
                string key = LayeredCacheKeys.GetGroupCategoriesKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupCategoriesInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<Dictionary<long, CategoryParentCache>>(key,
                                                                            ref groupCategoriesIds,
                                                                            BuildGroupCategories,
                                                                            new Dictionary<string, object>() { { "groupId", groupId } },
                                                                            groupId,
                                                                            LayeredCacheConfigNames.GET_GROUP_CATEGORIES,
                                                                            new List<string>() { invalidationKey }))
                {
                    log.Error($"Failed getting GetGroupCategories from LayeredCache, groupId: {groupId}, key: {key}");
                    return result;
                }

                if (groupCategoriesIds != null)
                {
                    if (ids?.Count > 0)
                    {
                        foreach (long item in ids)
                        {
                            if (groupCategoriesIds.ContainsKey(item))
                            {
                                result.Add(item, groupCategoriesIds[item]);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in groupCategoriesIds)
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }

                    if (result?.Count > 0 && rootOnly)
                    {
                        result = result.Where(x => x.Value.ParentId == 0).ToDictionary(y => y.Key, z => z.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetGroupCategories, groupId: {groupId}", ex);
            }

            return result;
        }

        internal static bool IsCategoryExist(int groupId, long id)
        {
            var categories = GetGroupCategoriesIds(groupId);
            if (categories == null || !categories.ContainsKey(id))
            {
                return false;
            }

            return true;
        }

        internal static CategoryItem GetCategoryItem(int groupId, long id)
        {
            CategoryItem result = null;

            try
            {
                string key = LayeredCacheKeys.GetCategoryItemKey(groupId, id);
                string invalidationKey = LayeredCacheKeys.GetCategoryIdInvalidationKey(id);
                if (!LayeredCache.Instance.Get<CategoryItem>(key,
                                                            ref result,
                                                        BuildCategoryItem,
                                                        new Dictionary<string, object>() { { "groupId", groupId }, { "id", id } },
                                                        groupId,
                                                        LayeredCacheConfigNames.GET_CATEGORY_ITEM,
                                                        new List<string>() { invalidationKey }))
                {
                    log.Error($"Failed getting GetCategoryItem from LayeredCache, groupId: {groupId}, key: {key}");
                }

            }
            catch (Exception ex)
            {
                log.Error($"Failed GetCategoryItem, groupId: {groupId}", ex);
            }

            return result;
        }

        internal static List<long> GetCategoryItemAncestors(int groupId, long id)
        {
            List<long> ancestors = new List<long>();

            var categories = GetGroupCategoriesIds(groupId);
            if (categories?.Count > 0 && categories.ContainsKey(id))
            {
                long parentId = categories[id].ParentId;
                while (parentId > 0)
                {
                    if (categories.ContainsKey(parentId))
                    {
                        if (ancestors.Contains(parentId))
                        {
                            ancestors.Clear();
                            break;
                        }

                        ancestors.Add(parentId);
                        parentId = categories[parentId].ParentId;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ancestors;
        }

        internal static List<long> GetCategoryItemSuccessors(int groupId, long id)
        {
            List<long> successors = new List<long>();

            var categories = GetGroupCategoriesIds(groupId);
            GetCategoryItemSuccessors(categories, id, successors);

            return successors;
        }

        private static void GetCategoryItemSuccessors(Dictionary<long, CategoryParentCache> groupCategories, long id, List<long> successors)
        {
            var childs = groupCategories.Where(x => x.Value.ParentId == id).Select(y => y.Key).ToList();

            foreach (long item in childs)
            {
                successors.Add(item);
                GetCategoryItemSuccessors(groupCategories, item, successors);
            }
        }

        internal static Status HandleCategoryChildUpdate(int groupId, long id, List<long> newChildCategoriesIds, List<long> oldChildCategoriesIds,
            ref List<long> categoriesToRemove, out bool updateChildCategories)
        {
            updateChildCategories = false;
            categoriesToRemove = new List<long>();

            if (newChildCategoriesIds == null)
            {
                return new Status(eResponseStatus.OK);
            }

            if (newChildCategoriesIds.Count == 0)
            {
                categoriesToRemove = oldChildCategoriesIds;
                return new Status(eResponseStatus.OK);
            }
            else
            {
                if (newChildCategoriesIds.Contains(id))
                {
                    return new Status(eResponseStatus.ChildCategoryCannotBeTheCategoryItself, "A child category cannot be the category itself.");
                }

                //validate ChildCategoriesIds
                var groupCategories = GetGroupCategoriesIds(groupId, newChildCategoriesIds);

                if (groupCategories == null || groupCategories.Count != newChildCategoriesIds.Count)
                {
                    return new Status(eResponseStatus.ChildCategoryNotExist, "Child Category does not exist.");
                }

                foreach (var item in groupCategories)
                {
                    if (item.Value.ParentId == 0)
                    {
                        var successors = GetCategoryItemSuccessors(groupId, item.Key);
                        if (successors.Contains(id))
                        {
                            return new Status(eResponseStatus.ParentIdShouldNotPointToItself, "Circle alert!!!!");
                        }
                    }

                    if (item.Value.ParentId > 0 && item.Value.ParentId != id)
                    {
                        return new Status(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory, $"Child Category contains other parent. id = {item.Key}");
                    }
                }

                if (oldChildCategoriesIds.Count == 0)
                {
                    updateChildCategories = true;
                }
                else
                {
                    Dictionary<long, int> ccim = new Dictionary<long, int>();
                    for (int i = 0; i < newChildCategoriesIds.Count; i++)
                    {
                        long item = newChildCategoriesIds[i];
                        ccim.Add(item, i);
                    }

                    for (int i = 0; i < oldChildCategoriesIds.Count; i++)
                    {
                        long item = oldChildCategoriesIds[i];

                        if (!ccim.ContainsKey(item))
                        {
                            categoriesToRemove.Add(item);
                        }
                        else if (ccim[item] != i)
                        {
                            updateChildCategories = true;
                        }
                    }
                }
            }

            return new Status(eResponseStatus.OK);
        }

        internal static bool Add(int groupId, long userId, CategoryItem objectToAdd)
        {
            bool result = false;
            try
            {
                List<KeyValuePair<long, int>> channels = null;

                if (objectToAdd.UnifiedChannels?.Count > 0)
                {
                    channels = objectToAdd.UnifiedChannels.Select(x => new KeyValuePair<long, int>(x.Id, (int)x.Type)).ToList();
                }

                //set NamesInOtherLanguages
                var status = HandleNamesInOtherLanguages(groupId, objectToAdd.NamesInOtherLanguages, out List<KeyValuePair<long, string>> languageCodeToName);

                if (!status.IsOkStatusCode())
                {
                    log.Error($"Error while HandleNamesInOtherLanguages");
                    return result;
                }

                long id = CatalogDAL.InsertCategory(groupId, userId, objectToAdd.Name, languageCodeToName, channels, objectToAdd.DynamicData);

                if (id == 0)
                {
                    log.Error($"Error while InsertCategory");
                    return result;
                }

                // set child category's order
                bool invalidateChilds = false;
                if (objectToAdd.ChildrenIds?.Count > 0)
                {
                    if (!CatalogDAL.UpdateCategoryOrderNum(groupId, userId, id, objectToAdd.ChildrenIds))
                    {
                        log.Error($"Error while order child categories. new categoryId: {id}");
                    }
                    else
                    {
                        invalidateChilds = true;
                    }
                }

                // Add VirtualAssetInfo for new category 
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Category,
                    Id = id,
                    Name = objectToAdd.Name,
                    UserId = userId
                };

                api.AddVirtualAsset(groupId, virtualAssetInfo);

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(groupId));
                if (invalidateChilds)
                {
                    foreach (var item in objectToAdd.ChildrenIds)
                    {
                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(item));
                    }
                }

                objectToAdd.Id = id;
                result = true;
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem add. Name:{objectToAdd.Name}", ex);
            }

            return result;
        }

        internal static Status HandleNamesInOtherLanguages(int groupId, List<LanguageContainer> namesInOtherLanguages, out List<KeyValuePair<long, string>> languageCodeToName)
        {
            languageCodeToName = null;
            if (namesInOtherLanguages != null && namesInOtherLanguages.Count > 0)
            {
                CatalogGroupCache catalogGroupCache = null;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling HandleNamesInOtherLanguages");
                    return new Status(eResponseStatus.Error);
                }

                languageCodeToName = new List<KeyValuePair<long, string>>();
                foreach (LanguageContainer language in namesInOtherLanguages)
                {
                    languageCodeToName.Add(new KeyValuePair<long, string>(catalogGroupCache.LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue));
                }
            }

            return new Status(eResponseStatus.OK);
        }
    }
}