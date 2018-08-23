using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DAL;
using CouchbaseManager;

namespace APILogic.Api.Managers
{

    public class PersonalListManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const eCouchbaseBucket COUCHBASE_BUCKET = eCouchbaseBucket.NOTIFICATION;

        public static GenericResponse<PersonalListItem> AddPersonalListItem(int groupId, long userId, PersonalListItem personalListItem)
        {
            GenericResponse<PersonalListItem> response = new GenericResponse<PersonalListItem>();

            UserPersonalList userPersonalList = GetUserPersonalListCB(userId);
            int itemId = 0;
            personalListItem.Timestamp = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);

            if (userPersonalList == null)
            {
                userPersonalList = new UserPersonalList(userId);
                userPersonalList.CreateDateSec = personalListItem.Timestamp;
            }

            if (userPersonalList.Items != null && userPersonalList.Items.Count > 0)
            {
                if (userPersonalList.Items.Count(x => x.PartnerListType == personalListItem.PartnerListType && x.Ksql.Equals(personalListItem.Ksql)) > 0)
                {
                    //allready exist
                }

                itemId = userPersonalList.Items.Max(x => x.Id);
            }

            personalListItem.Id = itemId + 1;
            userPersonalList.Items.Add(personalListItem);
            
            string key = GetUserPersonalListKey(userId);
            if (!UtilsDal.SaveObjectInCB<UserPersonalList>(COUCHBASE_BUCKET, key, userPersonalList))
            {
                //log fail to delete
            }
            else
            {
                response.Object = personalListItem;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public static GenericResponse<List<PersonalListItem>> GetUserPersonalListItems(int groupId, long userId, int pageIndex, int pageSize, OrderDiretion order, HashSet<int> partnerListTypes)
        {
            GenericResponse<List<PersonalListItem>> response = new GenericResponse<List<PersonalListItem>>();
            List<PersonalListItem> items = new List<PersonalListItem>();

            UserPersonalList userPersonalList = GetUserPersonalListCB(userId);

            if (userPersonalList == null)
            {
                return response;
            }

            if (userPersonalList.Items != null && userPersonalList.Items.Count > 0)
            {
                if (partnerListTypes != null && partnerListTypes.Count > 0)
                {
                    items = userPersonalList.Items.Where(x => partnerListTypes.Contains(x.PartnerListType)).ToList();
                }
                else
                {
                    items = userPersonalList.Items;
                }

                if (items != null && items.Count > 0)
                {
                    if (order == OrderDiretion.Desc)
                    {
                        items.Reverse();
                    }

                    if (pageSize > 0)
                    {
                        items = items.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                    }
                }
            }

            response.Object = items;
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        public static Status DeletePersonalListItem(int groupId, long userId, long personalListItemId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                UserPersonalList userPersonalList = GetUserPersonalListCB(userId);

                if (userPersonalList == null || userPersonalList.Items == null || userPersonalList.Items.Count == 0)
                {
                    //log
                    return response;
                }

                PersonalListItem itemToDelete = userPersonalList.Items.FirstOrDefault(x => x.Id == personalListItemId);
                if (itemToDelete == null)
                {
                    //log item not found
                }
                else
                {
                    if (!userPersonalList.Items.Remove(itemToDelete))
                    {
                        //log fail to delete
                    }

                    string key = GetUserPersonalListKey(userId);

                    if (userPersonalList.Items.Count == 0)
                    {
                        UtilsDal.DeleteObjectFromCB(COUCHBASE_BUCKET, key);
                    }
                    else
                    {
                        if (!UtilsDal.SaveObjectInCB<UserPersonalList>(COUCHBASE_BUCKET, key, userPersonalList))
                        {
                            //log fail to delete
                        }
                    }

                    response.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                //log.ErrorFormat("", ex, groupId, assetRuleId);
            }

            return response;
        }

        private static string GetUserPersonalListKey(long userId)
        {
            return string.Format("userPersonalList:{0}", userId);
        }

        private static UserPersonalList GetUserPersonalListCB(long userId)
        {
            string key = GetUserPersonalListKey(userId);
            return UtilsDal.GetObjectFromCB<UserPersonalList>(COUCHBASE_BUCKET, key);
        }

    }
}
