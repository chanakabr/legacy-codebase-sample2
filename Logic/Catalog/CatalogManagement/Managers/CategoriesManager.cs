using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;
using MetaType = ApiObjects.MetaType;

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
                        ParentCategoryId = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "PARENT_CATEGORY_ID"),
                        Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "CATEGORY_NAME"),
                        HasDynamicData = ODBCWrapper.Utils.ExtractBoolean(ds.Tables[0].Rows[0], "HAS_METADATA"),
                    };

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

                    // Set ChildCategoriesIds
                    var groupCategoriesIds = GetGroupCategoriesIds(groupId);
                    if (groupCategoriesIds != null)
                    {
                        categoryItem.ChildCategoriesIds = groupCategoriesIds.Where(x => x.Value.ParentId == id).OrderBy(y => y.Value.Order).Select(z => z.Key).ToList();
                    }

                    if (categoryItem.HasDynamicData)
                    {
                        categoryItem.DynamicData = CatalogDAL.GetCategoryDynamicData(id);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while getting GetCategoryItembyDb. group id = {groupId}", ex);
            }

            return categoryItem;
        }

        internal static Dictionary<long, CategoryParentCache> GetGroupCategoriesIds(int groupId, List<long> ids = null)
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
                        result = groupCategoriesIds;
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
                string invalidationKey = LayeredCacheKeys.GetCategoryIdInvalidationKey(groupId);
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
                
                if (result != null)
                {
                    result.ParentCategoryId = null;
                    var groupCategories = GetGroupCategoriesIds(groupId, new List<long>() { id });
                    if (groupCategories.ContainsKey(id) && groupCategories[id].ParentId > 0)
                    {
                        result.ParentCategoryId = groupCategories[id].ParentId;
                    }
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
            
            /*
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
                        parentId = 
                        id = categories[id].ParentId;
                    }
                    else
                    {
                        break;
                    }

                }
            }
            */

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

        internal static Status HandleCategoryChildUpdate(int groupId, long id, List<long> newChildCategoriesIds, List<long> oldChildCategoriesIds, ref List<long> categoriesToRemove, out bool updateChildCategories)
        {
            Status status = new Status();
            updateChildCategories = false;
            categoriesToRemove = new List<long>();
            
            if (newChildCategoriesIds != null)
            {
                if (newChildCategoriesIds.Count == 0)
                {
                    categoriesToRemove = oldChildCategoriesIds;
                }
                else
                {
                    //validate ChildCategoriesIds
                    var groupCategories = GetGroupCategoriesIds(groupId, newChildCategoriesIds);

                    if (groupCategories == null || groupCategories.Count != newChildCategoriesIds.Count)
                    {   
                        return new Status(eResponseStatus.Error, $"Child Category does not exist.");
                    }

                    foreach (var item in groupCategories)
                    {
                        if (item.Value.ParentId == 0)
                        {
                            var successors = GetCategoryItemSuccessors(groupId, item.Key);
                            if (successors.Contains(id))
                            {
                                //TODO error
                                return new Status(eResponseStatus.Error, $"Circle alert!!!!");
                            }
                        }

                        if (item.Value.ParentId > 0 && item.Value.ParentId != id)
                        {
                            //TODO error
                            return new Status(eResponseStatus.Error, $"Child Category contains other parent. id = {item.Key}");
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
            }

            return status;
        }
    }
}
