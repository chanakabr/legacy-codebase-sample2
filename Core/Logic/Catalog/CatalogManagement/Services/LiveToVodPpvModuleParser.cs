using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using Core.Pricing;
using Microsoft.Extensions.Logging;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LiveToVodPpvModuleParser : ILiveToVodPpvModuleParser
    {
        private const string L2VPpvMeta = "l2v_ppv_module";
        private const string L2VPpvMetaDateFormat = "dd/MM/yyyy HH:mm:ss";

        private readonly IPpvManager _ppvManager;
        private readonly ILogger<LiveToVodPpvModuleParser> _logger;

        public LiveToVodPpvModuleParser(IPpvManager ppvManager, ILogger<LiveToVodPpvModuleParser> logger)
        {
            _ppvManager = ppvManager;
            _logger = logger;
        }

        /// <summary>
        /// Finds meta with name 'l2v_ppv_module' and parses it's value.
        /// Meta value must be in a specific format, for example:
        /// "{file_type_id1};{PPV_module_name1},{file_type_id2};{PPV_module_name2};{start_date1?;{end_date1}"
        /// </summary>
        /// <param name="asset">Program asset received with a CRUD event.</param>
        /// <returns>PPV modules to apply for the current l2v asset</returns>
        public IEnumerable<PpvModuleInfo> GetParsedPpv(ProgramAsset asset)
        {
            var ppvMeta = asset.Metas?.FirstOrDefault(x =>
                x.Name?.Equals(L2VPpvMeta, StringComparison.OrdinalIgnoreCase) == true);
            if (ppvMeta == null)
            {
                return Enumerable.Empty<PpvModuleInfo>();
            }

            var ppvModulesInfo = ppvMeta.Value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (ppvModulesInfo == null || ppvModulesInfo.Length == 0)
            {
                _logger.LogWarning("PPV meta format is incorrect. groupId:[{PartnerId}]. assetId:[{AssetId}].]",
                    asset.PartnerId,
                    asset.Id);

                return Enumerable.Empty<PpvModuleInfo>();
            }

            ContextData contextData = new ContextData((int)asset.PartnerId);
            var ppvListResponse = _ppvManager.GetPPVModules(contextData);
            if (!ppvListResponse.IsOkStatusCode())
            {
                return Enumerable.Empty<PpvModuleInfo>();
            }

            var ppvModulesAdded = new Dictionary<(long, long), PpvModuleInfo>();
            foreach (var ppvModuleInfo in ppvModulesInfo)
            {
                if (TryParsePpvModuleInfo(asset.PartnerId, asset.Id, ppvModuleInfo, ppvListResponse.Objects, out var ppvInfo)
                    && !ppvModulesAdded.ContainsKey((ppvInfo.PpvModuleId, ppvInfo.FileTypeId)))
                {
                    // avoiding duplications by ppv module + file type
                    ppvModulesAdded.Add((ppvInfo.PpvModuleId, ppvInfo.FileTypeId), ppvInfo);
                }
            }

            return ppvModulesAdded.Values;
        }

        private bool TryParsePpvModuleInfo(
            long partnerId,
            long assetId,
            string ppvModuleInfo,
            IEnumerable<PPVModule> ppvModules,
            out PpvModuleInfo result)
        {
            result = null;
            var ppvModuleValues = ppvModuleInfo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (ppvModuleValues.Length < 2
                || !long.TryParse(ppvModuleValues[0], out var fileTypeId))
            {
                _logger.LogWarning("PPV meta format is incorrect. partnerId:[{PartnerId}]. assetId:[{AssetId}].]",
                    partnerId,
                    assetId);

                return false;
            }

            var ppvModule = ppvModules.FirstOrDefault(
                x => x.m_sObjectVirtualName.Equals(ppvModuleValues[1], StringComparison.OrdinalIgnoreCase));
            if (ppvModule == null || !long.TryParse(ppvModule.m_sObjectCode, out var ppvModuleId))
            {
                _logger.LogWarning("PPV module not found. partnerId:[{PartnerId}]. assetId:[{AssetId}]. ppv module name: [{moduleName}]",
                    partnerId,
                    assetId,
                    ppvModuleValues[1]);

                return false;
            }

            result = new PpvModuleInfo { PpvModuleId = ppvModuleId, FileTypeId = fileTypeId };
            if (ppvModuleValues.Length == 4
                && DateTime.TryParseExact(ppvModuleValues[2], L2VPpvMetaDateFormat, null, DateTimeStyles.None, out var startDate)
                && DateTime.TryParseExact(ppvModuleValues[3], L2VPpvMetaDateFormat, null, DateTimeStyles.None, out var endDate))
            {
                result.StartDate = startDate;
                result.EndDate = endDate;
            }

            return true;
        }
    }
}