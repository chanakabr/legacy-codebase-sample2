using ApiObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.Notification
{
    public class TopicInterestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public static ApiObjects.Response.Status AddUserInterest(int partnerId, int userId, UserInterest userInterest)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            UserInterests userInterests = null;
            bool updateIsNeeded = false;
            try
            {
                // Get user interests 
                userInterests = InterestDal.GetUserInterest(partnerId, userId);
                if (userInterests == null)
                {
                    userInterests = new UserInterests() { PartnerId = partnerId, UserId = userId };
                    userInterests.UserInterestList.Add(userInterest);
                    updateIsNeeded = true;
                }
                else
                {
                    // Update with new interest
                    string userInterestItemString = string.Empty;
                    string newUserInterestString = JsonConvert.SerializeObject(userInterest);
                    foreach (var UserInterestItem in userInterests.UserInterestList)
                    {
                        userInterestItemString = JsonConvert.SerializeObject(UserInterestItem);
                        if (!newUserInterestString.Equals(userInterestItemString))
                        {
                            userInterests.UserInterestList.Add(userInterest);
                            updateIsNeeded = true;
                        }
                    }
                }

                // Set CB with new interest
                if (updateIsNeeded)
                {
                    if (!InterestDal.SetUserInterest(userInterests))
                        log.ErrorFormat("Error inserting user interest  into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));
                }

                response.Code = (int)eResponseStatus.OK;
                response.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return response;
        }

        internal static UserInterestResponseList GetUserInterests(int groupId, int userId)
        {
            UserInterestResponseList response = new UserInterestResponseList() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
            UserInterests userInterests = null;

            try
            {
                // insert user interest to CB
                userInterests = InterestDal.GetUserInterest(groupId, userId);
                if (userInterests == null)
                    log.ErrorFormat("Error inserting user interest  into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));

                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();
                response.UserInterests = userInterests.UserInterestList;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return response;
        }
    }
}
