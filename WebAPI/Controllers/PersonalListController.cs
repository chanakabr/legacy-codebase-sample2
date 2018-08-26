using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Api;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("personalList")]
    public class PersonalListController : IKalturaController
    {
        /// <summary>
        /// List user's tv personal item to follow.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="filter">Personal list filter</param>
        /// <param name="pager">pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidUser)]
        static public KalturaPersonalListListResponse List(KalturaPersonalListFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaPersonalListListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (filter == null)
                filter = new KalturaPersonalListFilter();

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int userId = 0;
                if (!int.TryParse(userID, out userId))
                {
                    throw new ClientException((int)eResponseStatus.InvalidUser, "Invalid userId");
                }

                response = ClientsManager.ApiClient().GetPersonalListItems(groupId, userId, pager.PageSize.Value, pager.PageIndex.Value, filter.OrderBy, filter.GetPartnerListTypeIn());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        /// <summary>
        /// Add a user's personal list item to follow.
        /// </summary>
        /// <param name="personalList">Follow personal list item request parameters</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserAlreadyFollowing)]
        [Throws(eResponseStatus.InvalidUser)]
        static public KalturaPersonalList Add(KalturaPersonalList personalList)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            try
            {
                int userId = 0;
                if (!int.TryParse(userID, out userId))
                {
                    throw new ClientException((int)eResponseStatus.InvalidUser, "Invalid Username");
                }
                
                if (string.IsNullOrEmpty(personalList.Name) || string.IsNullOrWhiteSpace(personalList.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }

                if (string.IsNullOrEmpty(personalList.Ksql) || string.IsNullOrWhiteSpace(personalList.Ksql))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
                }
                
                return ClientsManager.ApiClient().AddPersonalListItemToUser(groupId, userId, personalList);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Remove followed item from user's personal list 
        /// </summary>
        /// <param name="personalListId">personalListId identifier</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("personalListId", MinLong = 1)]
        [Throws(eResponseStatus.UserNotFollowing)]
        [Throws(eResponseStatus.InvalidUser)]
        static public void Delete(long personalListId)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            try
            {
                int userId = 0;
                if (!int.TryParse(userID, out userId))
                {
                    throw new ClientException((int)eResponseStatus.InvalidUser, "Invalid Username");
                }

                ClientsManager.ApiClient().DeletePersonalListItemFromUser(groupId, userId, personalListId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}