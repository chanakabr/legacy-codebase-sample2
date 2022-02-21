using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.ModelsValidators;

namespace WebAPI.Controllers
{
    [Service("passwordPolicy")]
    public class PasswordPolicyController : IKalturaController
    {
        /// <summary>
        /// Add new KalturaPasswordPolicy
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objectToAdd">KalturaPasswordPolicy Object to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RoleDoesNotExists)]
        static public KalturaPasswordPolicy Add(KalturaPasswordPolicy objectToAdd)
        {
            var contextData = KS.GetContextData();

            objectToAdd.ValidateForAdd();

            // call to manager and get response
            
            Func<PasswordPolicy, GenericResponse<PasswordPolicy>> addFunc = (PasswordPolicy coreObject) =>
                PasswordPolicyManager.Instance.Add(contextData, coreObject);

            var response = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            
            return response;
        }

        /// <summary>
        /// Update existing KalturaPasswordPolicy
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of KalturaPasswordPolicy to update</param>
        /// <param name="objectToUpdate">KalturaPasswordPolicy Object to update</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.PasswordPolicyDoesNotExist)]
        [Throws(eResponseStatus.RoleDoesNotExists)]
        static public KalturaPasswordPolicy Update(long id, KalturaPasswordPolicy objectToUpdate)
        {
            objectToUpdate.ValidateForUpdate();
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;

            // call to manager and get response
            Func<PasswordPolicy, GenericResponse<PasswordPolicy>> coreFunc = (PasswordPolicy coreObject) =>
                 PasswordPolicyManager.Instance.Update(contextData, coreObject);

            var response = ClientUtils.GetResponseFromWS(objectToUpdate, coreFunc);
            
            return response;
        }

        /// <summary>
        /// Delete existing PasswordPolicy
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">PasswordPolicy identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.PasswordPolicyDoesNotExist)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => PasswordPolicyManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Returns the list of available KalturaPasswordPolicy
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPasswordPolicyListResponse List(KalturaPasswordPolicyFilter filter = null)
        {
            var contextData = KS.GetContextData();
            if (filter != null)
            {
                filter = new KalturaPasswordPolicyFilter();
            }

            var coreFilter = AutoMapper.Mapper.Map<PasswordPolicyFilter>(filter);

            Func<GenericListResponse<PasswordPolicy>> listFunc = () =>
                PasswordPolicyManager.Instance.List(contextData, coreFilter);

            KalturaGenericListResponse<KalturaPasswordPolicy> result =
               ClientUtils.GetResponseListFromWS<KalturaPasswordPolicy, PasswordPolicy>(listFunc);

            var response = new KalturaPasswordPolicyListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }
    }
}