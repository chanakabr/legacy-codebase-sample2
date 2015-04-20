using System;
using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using RestfulTVPApi.Catalog;
using RestfulTVPApi.Objects.Responses;


namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/epg/auto_complete/{search_text}", "GET", Notes = "This method returns a string array of EPG program names that starts with the given search text")]
    public class GetEPGAutoCompleteRequest : PagingRequest, IReturn<List<string>>
    {
        [ApiMember(Name = "search_text", Description = "Search Text", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string search_text { get; set; }
    }

    [Route("/epg/channels", "GET", Notes = "This method returns an array of EPG Channels for a specific account")]
    public class GetEPGChannelsRequest : PagingRequest, IReturn<List<RestfulTVPApi.Api.EPGChannelObject>>
    {
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = "OrderBy", IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(TVPApiModule.Context.OrderBy))]
        public TVPApiModule.Context.OrderBy order_by { get; set; }
    }

    [Route("/epg/programs/{program_id}/comments", "GET", Notes = "This method returns a list of EPG comments created by users")]
    public class GetEPGCommentsListRequest : PagingRequest, IReturn<List<EPGComment>>
    {
        [ApiMember(Name = "program_id", Description = "Program ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int program_id { get; set; }
    }

    [Route("/epg/channels/{channel_ids}", "GET", Notes = "This method returns an array of EPG channel programs, for each EPG channel entered, and which is available for the time range entered. This method is usually followed by GetEPGChannels")]
    public class GetEPGMultiChannelProgramRequest : PagingRequest, IReturn<List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject>>
    {
        [ApiMember(Name = "channel_ids", Description = "Channels IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] channel_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "unit", Description = "Program ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("unit", typeof(RestfulTVPApi.Objects.RequestModels.Enums.EPGUnit))]
        public RestfulTVPApi.Objects.RequestModels.Enums.EPGUnit unit { get; set; }
        [ApiMember(Name = "from_offset", Description = "From Offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int from_offset { get; set; }
        [ApiMember(Name = "to_offset", Description = "To Offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int to_offset { get; set; }
        [ApiMember(Name = "utc_offset", Description = "UTC Offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int utc_offset { get; set; }
    }

    [Route("/epg/programs", "GET", Notes = "This method searches the EPG programs by search text")]
    public class SearchEPGProgramsRequest : PagingRequest, IReturn<List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>>
    {
        [ApiMember(Name = "search_text", Description = "Search Text", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string search_text { get; set; }
    }

    //Need to understand what this does to decide on the route
    [Route("/epg/license_link", "GET", Notes = "Returns playable link to requested media. Playable link is a function of user requests (base link, mediaFileID, start time, start position, etc.,). Response includes a URL which is then sent to the player")]
    public class GetEPGLicensedLinkRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "epg_item_id", Description = "EPG Item ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int epg_item_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "start_time", Description = "User's Identifiers", ParameterType = "query", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime start_time { get; set; }
        [ApiMember(Name = "base_link", Description = "Base Link", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string base_link { get; set; }
        [ApiMember(Name = "refferer", Description = "Refferer", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string refferer { get; set; }
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "format_type", Description = "Format Type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int format_type { get; set; }

    }

    [Route("/epg/programs/{program_id}/rules", "GET", Notes = "This method returns an array containing the EPG program’s rules. These are the same rules as GetGroupRules.")]
    public class GetEPGProgramRulesRequest : RequestBase, IReturn<List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>>
    {
        [ApiMember(Name = "program_id", Description = "Program ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int program_id { get; set; }
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/epg/programs_by_and_ol_list", "GET", Notes = "Search Epg using an 'Or' list and an 'and' list. Key-Value pairs of tags and metas are expected in the lists. Between the two lists an AND logic will be implemented.")]
    public class SearchEPGByAndOrListRequest : PagingRequest, IReturn<List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>>
    {
        [ApiMember(Name = "and_list", Description = "And key value pairs", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> and_list { get; set; }
        [ApiMember(Name = "or_list", Description = "Or key value pairs", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> or_list { get; set; }
    }

    [Route("/epg/{epg_channel_id}/channels_program/from/{from_offset}/to/{to_offset}/utc_offset/{utc_offset}", "GET", Notes = "Gets an array of EPG channel programs")]
    public class GetEPGChannelsProgramsRequest : RequestBase, IReturn<List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>>
    {
        [ApiMember(Name = "epg_channel_id", Description = "EPG Channel Identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int epg_channel_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Picture Size. Options: 'full' - original picture size width x height for all the rest", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "unit", Description = "Days,Hours,Current", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public RestfulTVPApi.Objects.RequestModels.Enums.EPGUnit unit { get; set; }
        [ApiMember(Name = "from_offset", Description = "From offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int from_offset { get; set; }
        [ApiMember(Name = "to_offset", Description = "To offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int to_offset { get; set; }
        [ApiMember(Name = "utc_offset", Description = "To offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int utc_offset { get; set; }
    }

    #endregion

    #region PUT
    #endregion

    #region POST
    #endregion

    #region DELETE
    #endregion
}