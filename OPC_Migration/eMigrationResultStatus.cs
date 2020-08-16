using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_Migration
{
    public enum eMigrationResultStatus
    {
        OK = 0,
        FailedValidationOfPicSizes = 1,
        FailedValidationOfGroupRatios = 2,
        FailedValidationOfGroupImageTypes = 3,
        FailedValidationOfGroupFileTypes = 4,
        FailedValidationOfGroupTopics = 5,
        FailedValidationOfGroupAssetStructs = 6,
        FailedValidationOfAssetStructToTopicsMapping = 7,
        FailedValidationOfAssets = 8,
        FailedValidationOfChannels = 9,
        ClearAllCachesFailed = 10,
        UpdateGroupPicsIdsFailed = 11,
        UpdateGroupIdInNeededTablesFailed = 12,
        InsertGroupRatiosFailed = 13,
        InsertGroupImageTypesFailed = 14,
        UpdateGroupMediaFileTypesFailed = 15,
        InsertGroupTopicsFailed = 16,
        UpdateTagTableWithTopicIdsFailed = 17,
        UpdateParentalRulesTableWithTopicIdsFailed = 18,
        AddOrUpdateGroupAssetStructsFailed = 19,
        UpdateGroupMediaAssetsFailed = 20,
        AddImagesToAssetFailed = 21,
        FailedValidationOfPicSizesFailed = 22,
        UpdatePicsContentIdBaseUrlFailed = 23,
        UpdateGroupMediaAssetsFilesFailed = 24,
        UpdateGroupChannelsFailed = 25,
        UpdateDuplicateTagValuesAndTagTranslationsFailed = 26,
        FailedValidationOfLanguagesPicSizes = 27,
        UpdateGroupExtraLanguagesFailed = 28,
        FailedGettingRollbackTablesInfo = 29,
        FailedRollBackOfNewOpcTables = 30,
        GetSequenceIdForBackupFailed = 31,
        FailedCreateSnapshot = 32,
        BackupOfPartialTablesFailed = 33,
        FailedRollbackOfAllTables = 34,
        BackupOfFullTablesFailed = 35,
        UpdateMediaConcurrencyRulesTableWithTopicIdsFailed = 36
    }
}
