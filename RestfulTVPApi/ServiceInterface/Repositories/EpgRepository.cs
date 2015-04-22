using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Catalog;


namespace RestfulTVPApi.ServiceInterface
{
    public class EpgRepository : IEpgRepository
    {
        #region CONSTS

        private const string EPG_SEARCH_OFFSET_INITIAL_TCM_KEY = "EPGValues";

        #endregion

        public List<string> GetEPGAutoComplete(GetEPGAutoCompleteRequest request)
        {
            List<string> retVal = null;

            DateTime startTime;
            DateTime endTime;

            int nEPGSearchOffsetDays = TCMClient.Settings.Instance.GetValue<int>(string.Format("{0}.{1}", EPG_SEARCH_OFFSET_INITIAL_TCM_KEY, "EPGSearchOffsetDays"));

            startTime = DateTime.UtcNow.AddDays(-nEPGSearchOffsetDays);
            endTime = DateTime.UtcNow.AddDays(nEPGSearchOffsetDays);

            //retVal = new APIEPGAutoCompleteLoader(request.GroupID, request.InitObj.Platform, request.InitObj.UDID, Utils.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.search_text, startTime, endTime)
            //{
            //    Culture = request.InitObj.Locale.LocaleLanguage
            //}.Execute() as List<string>;

            return retVal;
        }

        public List<EPGChannel> GetEPGChannels(GetEPGChannelsRequest request)
        {
            return ClientsManager.ApiClient().GetEPGChannel(request.pic_size);            
        }

        public List<EPGComment> GetEPGCommentsList(GetEPGCommentsListRequest request)
        {
            //return CommentHelper.GetEPGCommentsList(request.GroupID, request.InitObj.Platform, request.InitObj.Locale.LocaleLanguage, request.program_id, request.page_size, request.page_number);            
            return null;
        }

        public List<EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(GetEPGMultiChannelProgramRequest request)
        {
            List<EPGMultiChannelProgrammeObject> sRet = null;

            //EPGLoader loader;
            //    List<int> channelIDs = request.channel_ids.Select(c => int.Parse(c)).ToList();
            //    switch (request.unit)
            //    {
            //        case EPGUnit.Days:
            //            loader = new EPGLoader(request.GroupID, Utils.GetClientIP(), 0, 0, channelIDs, EpgSearchType.ByDate, DateTimeOffset.UtcNow.AddDays(request.from_offset).DateTime, DateTimeOffset.UtcNow.AddDays(request.to_offset).DateTime, 0, 0);
            //            break;
            //        case EPGUnit.Hours:
            //            loader = new EPGLoader(request.GroupID, Utils.GetClientIP(), 0, 0, channelIDs, EpgSearchType.ByDate, DateTimeOffset.UtcNow.AddHours(request.from_offset).DateTime, DateTimeOffset.UtcNow.AddHours(request.to_offset).DateTime, 0, 0);
            //            break;
            //        case EPGUnit.Current:
            //            loader = new EPGLoader(request.GroupID, Utils.GetClientIP(), 0, 0, channelIDs, EpgSearchType.Current, DateTime.UtcNow, DateTime.UtcNow, request.from_offset, request.to_offset);
            //            break;
            //        default:
            //            loader = new EPGLoader(request.GroupID, Utils.GetClientIP(), 0, 0, channelIDs, EpgSearchType.Current, DateTime.UtcNow, DateTime.UtcNow, request.from_offset, request.to_offset);
            //            break;
            //    }

            //    loader.DeviceId = request.InitObj.UDID;
            //    loader.SiteGuid = request.InitObj.SiteGuid;
            //    sRet = (loader.Execute() as List<TVPPro.SiteManager.Objects.EPGMultiChannelProgrammeObject>).Select(p => p.ToApiObject()).ToList();
            
            return sRet;
        }

        public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> SearchEPGPrograms(SearchEPGProgramsRequest request)
        {
            List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> retVal = null;

            DateTime startTime;
            DateTime endTime;

            int nEPGSearchOffsetDays = TCMClient.Settings.Instance.GetValue<int>(string.Format("{0}.{1}", EPG_SEARCH_OFFSET_INITIAL_TCM_KEY, "EPGSearchOffsetDays"));

            startTime = DateTime.UtcNow.AddDays(-nEPGSearchOffsetDays);
            endTime = DateTime.UtcNow.AddDays(nEPGSearchOffsetDays);

            //retVal = new APIEPGSearchLoader(request.GroupID, request.InitObj.Platform, request.InitObj.UDID, Utils.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.search_text, startTime, endTime)
            //{
            //    Culture = request.InitObj.Locale.LocaleLanguage
            //}.Execute() as List<EPGChannelProgrammeObject>;

            return retVal;
        }

        public List<GroupRule> GetEPGProgramRules(GetEPGProgramRulesRequest request)
        {
            return ClientsManager.ApiClient().GetEPGProgramRules(request.media_id, request.program_id, request.site_guid, Utils.GetClientIP(), request.InitObj.UDID);
        }

        public string GetEPGLicensedLink(GetEPGLicensedLinkRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetEPGLicensedLink(request.site_guid, request.media_file_id, request.epg_item_id, request.start_time, request.base_link, Utils.GetClientIP(), request.refferer, request.country_code, request.language_code, request.device_name, request.format_type);
        }

        public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> SearchEPGByAndOrList(SearchEPGByAndOrListRequest request)
        {
            List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> retVal = null;

            DateTime startTime;
            DateTime endTime;

            int nEPGSearchOffsetDays = TCMClient.Settings.Instance.GetValue<int>(string.Format("{0}.{1}", EPG_SEARCH_OFFSET_INITIAL_TCM_KEY, "EPGSearchOffsetDays"));

            startTime = DateTime.UtcNow.AddDays(-nEPGSearchOffsetDays);
            endTime = DateTime.UtcNow.AddDays(nEPGSearchOffsetDays);

            //retVal = new APIEPGSearchLoader(request.GroupID, request.InitObj.Platform.ToString(), Utils.GetClientIP(), request.page_size, request.page_number, request.and_list, request.or_list, true, startTime, endTime)
            //{
            //    Culture = request.InitObj.Locale.LocaleLanguage
            //}.Execute() as List<EPGChannelProgrammeObject>;

            return retVal;
        }


        public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> GetEPGChannelsPrograms(GetEPGChannelsProgramsRequest request)
        {
            List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> channelPrograms = new List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>();
            //APIEPGLoader loader;
            List<int> channelIDs = new List<int>() { request.epg_channel_id };
            DateTime _offsetNow = DateTime.UtcNow.AddHours(request.utc_offset);
            string ip = Utils.GetClientIP();
            //switch (request.unit)
            //{
            //    case EPGUnit.Days:
            //        DateTime from = new DateTime(_offsetNow.Year, _offsetNow.Month, _offsetNow.Day, 0, 0, 0), to = new DateTime(_offsetNow.Year, _offsetNow.Month, _offsetNow.Day, 0, 0, 0);
            //        loader = new APIEPGLoader(request.GroupID, request.InitObj.Platform, ip, 0, 0, channelIDs, EpgSearchType.ByDate, from.AddDays(request.from_offset), to.AddDays(request.to_offset), 0, 0, request.InitObj.Locale.LocaleLanguage);
            //        break;
            //    case EPGUnit.Hours:
            //        loader = new APIEPGLoader(request.GroupID, request.InitObj.Platform, ip, 0, 0, channelIDs, EpgSearchType.ByDate, _offsetNow.AddHours(request.from_offset), _offsetNow.AddHours(request.to_offset), 0, 0, request.InitObj.Locale.LocaleLanguage);
            //        break;
            //    case EPGUnit.Current:
            //        loader = new APIEPGLoader(request.GroupID, request.InitObj.Platform, ip, 0, 0, channelIDs, EpgSearchType.Current, _offsetNow, _offsetNow, request.from_offset, request.to_offset, request.InitObj.Locale.LocaleLanguage);
            //        break;
            //    default:
            //        loader = new APIEPGLoader(request.GroupID, request.InitObj.Platform, ip, 0, 0, channelIDs, EpgSearchType.Current, _offsetNow, _offsetNow, request.from_offset, request.to_offset, request.InitObj.Locale.LocaleLanguage);
            //        break;
            //}

            //loader.DeviceId = request.InitObj.UDID;
            //loader.SiteGuid = request.InitObj.SiteGuid;

            //channelPrograms = loader.Execute() as List<EPGChannelProgrammeObject>;
            ////if (loaderRes != null && loaderRes.Count() > 0)
            ////    sRet = (loaderRes[0] as EPGMultiChannelProgrammeObject).EPGChannelProgrammeObject.ToArray();

            return channelPrograms;
        }
    }
}