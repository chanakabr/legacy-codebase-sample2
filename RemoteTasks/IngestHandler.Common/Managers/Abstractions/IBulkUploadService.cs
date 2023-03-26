using System.Threading.Tasks;

namespace IngestHandler.Common.Managers.Abstractions
{
    public interface IBulkUploadService
    {
        Task<bool> ShouldProcessLinearChannelOfBulkUpload(int partnerId, long bulkUploadId, long linearChannelId);
    }
}
