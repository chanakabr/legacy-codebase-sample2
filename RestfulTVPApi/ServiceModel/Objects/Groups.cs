using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceModel
{

    #region GET

    [Route("/groups/operators", "GET", Notes = "This method returns the list of group operators.")]
    public class GetGroupOperatorsRequest : RequestBase, IReturn<IEnumerable<GroupOperator>>
    {
        [ApiMember(Name = "scope", Description = "Scope", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string scope { get; set; }
    }

    [Route("/groups/rules", "GET", Notes = "This method returns all of the group’s rules. For example: Geo block, parental & purchase.")]
    public class GetGroupRulesRequest : RequestBase, IReturn<IEnumerable<GroupRule>> { }

    #endregion

    #region PUT
    #endregion

    #region POST

    

    #endregion

    #region DELETE
    #endregion

}