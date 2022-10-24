using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ApiLogic.IndexManager.Transaction;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Epg;
using Elasticsearch.Net;
using ElasticSearch.NEST;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RestSharp;
using TVinciShared;
using ESUtils = ElasticSearch.Common.Utils;

namespace ApiLogic.IndexManager.NestData
{
    [ElasticsearchType(RelationName = NestEpg.RELATION_NAME)]
    public class NestEpg : NestBaseAsset
    {
        public const string RELATION_NAME = "epg";
        #region DataMembers

        [PropertyName("epg_id")]
        public ulong EpgID { get; set; }

        [PropertyName("epg_identifier")]
        public string EpgIdentifier { get; set; }

        [PropertyName("external_id")]
        public string ExternalId { get; set; }

        [PropertyName("epg_channel_id")]
        public int ChannelID { get; set; }

        [Date(Name = "search_end_date")]
        public DateTime SearchEndDate { get; set; }

        [PropertyName("linear_media_id")]
        public long? LinearMediaId { get; set; }

        [PropertyName("date_routing")]
        public string DateRouting { get; set; }

        [PropertyName("enable_cdvr")]
        public int EnableCDVR { get; set; }

        [PropertyName("enable_catch_up")]
        public int EnableCatchUp { get; set; }

        [PropertyName("crid")]
        public string Crid { get; set; }

        [PropertyName("cb_document_id")]
        public string CouchbaseDocumentId { get; set; }

        [PropertyName("is_auto_fill")]
        public bool IsAutoFill { get; set; }

        [PropertyName("suppressed")]
        public string Suppressed { get; set; }

        [PropertyName("recording_id")]
        public long? RecordingId { get; set; }

        [PropertyName("regions")]
        public List<int> Regions { get; set; }

        [PropertyName("__expiration")]
        public long? Expiration { get; set; }

        [PropertyName("external_offer_ids")]
        public List<string> ExternalOfferIds { get; set; }

        [PropertyName("__documentTransactionalStatus")]
        public string DocumentTransactionalStatus { get; set; }

        public JoinField Transaction { get; set; }

        #endregion

        #region Ctor

        // used by autommaper for migration consumer
        public NestEpg() { }

        public NestEpg(EpgCB epgCb, int languageId, bool isOpc = false, bool withRouting = true,
            string esDateOnlyFormat = "", long? recordingId = null, long? expiryUnixTimeStamp = null)
        {
            Initialize(epgCb, isOpc, withRouting, esDateOnlyFormat, languageId, recordingId, expiryUnixTimeStamp);
        }

        #endregion

        #region Initialize

        private void Initialize(EpgCB epgCb, bool isOpc, bool withRouting, string esDateOnlyFormat, int languageId,
            long? recordingId, long? expiryUnixTimeStamp)
        {
            EpgID = epgCb.EpgID;
            GroupID = isOpc ? epgCb.ParentGroupID : epgCb.GroupID;
            ChannelID = epgCb.ChannelID;
            IsActive = epgCb.IsActive;
            StartDate = epgCb.StartDate;
            EndDate = epgCb.EndDate;
            CacheDate = DateTime.UtcNow;
            CreateDate = epgCb.CreateDate;
            UpdateDate = epgCb.UpdateDate;
            SearchEndDate = epgCb.SearchEndDate;
            Crid = epgCb.Crid;
            EpgIdentifier = epgCb.EpgIdentifier;
            ExternalId = epgCb.EpgIdentifier;
            CouchbaseDocumentId = epgCb.DocumentId;
            IsAutoFill = epgCb.IsAutoFill;
            EnableCDVR = epgCb.EnableCDVR;
            EnableCatchUp = epgCb.EnableCatchUp;
            Suppressed = epgCb.Suppressed;
            Regions = epgCb.regions;

            if (epgCb.LinearMediaId > 0)
            {
                LinearMediaId = epgCb.LinearMediaId;
            }

            if (withRouting)
            {
                DateRouting = epgCb.StartDate.ToUniversalTime().ToString(esDateOnlyFormat);
            }

            var metasDict = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var langCode = epgCb.Language;

            var metas = new Dictionary<string, HashSet<string>>();

            if (epgCb.Metas != null)
            {
                foreach (var epgCbMeta in epgCb.Metas.Where(x => !x.Key.IsNullOrEmptyOrWhiteSpace()))
                {
                    metas[epgCbMeta.Key.ToLower()] =
                        epgCbMeta.Value.Select(x => ESUtils.ReplaceDocumentReservedCharacters(x, false)).ToHashSet();
                }
            }

            metasDict.Add(langCode, new Dictionary<string, HashSet<string>>(metas));
            Metas = metasDict; //lang

            var tagsDict = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var tags = new Dictionary<string, HashSet<string>>();

            if (epgCb.Tags != null)
            {
                foreach (var tag in epgCb.Tags.Where(x => !x.Key.IsNullOrEmptyOrWhiteSpace()))
                {
                    tags[tag.Key.ToLower()] = tag.Value.Select(x => ESUtils.ReplaceDocumentReservedCharacters(x, false)).ToHashSet();
                }
            }

            tagsDict.Add(langCode, new Dictionary<string, HashSet<string>>(tags));
            Tags = tagsDict; //lang

            var nameDict = new Dictionary<string, string>();
            nameDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(epgCb.Name, false));
            NamesDictionary = nameDict; //lang

            var descriptionDict = new Dictionary<string, string>();
            descriptionDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(epgCb.Description, false));
            Description = descriptionDict; //lang

            Language = langCode;
            LanguageId = languageId;

            ulong assetId = this.EpgID;

            if (recordingId.HasValue)
            {
                RecordingId = recordingId.Value;
                assetId = (ulong)recordingId.Value;
            }

            if (expiryUnixTimeStamp.HasValue)
            {
                Expiration = expiryUnixTimeStamp.Value;
            }

            this.DocumentId = $"{assetId}_{this.Language}";

            ExternalOfferIds = epgCb.ExternalOfferIds ?? new List<string>();
        }

        #endregion

        internal EpgProgramBulkUploadObject ToEpgProgramBulkUploadObject()
        {
            var epgItem = new EpgProgramBulkUploadObject();
            epgItem.EpgExternalId = this.EpgIdentifier;
            epgItem.StartDate = this.StartDate;
            epgItem.EndDate = this.EndDate;
            epgItem.EpgId = this.EpgID;
            epgItem.IsAutoFill = this.IsAutoFill;
            epgItem.ChannelId = this.ChannelID;
            epgItem.LinearMediaId = this.LinearMediaId.HasValue ? this.LinearMediaId.Value : 0;
            epgItem.ParentGroupId = this.GroupID;
            epgItem.GroupId = this.GroupID;
            return epgItem;
        }

    }


    [ElasticsearchType(RelationName = NESTEpgTransaction.RELATION_NAME)]
    public class NESTEpgTransaction 
    {
        public const string RELATION_NAME = "epg_transaction";
        public JoinField Transaction { get; set; }
    }
}