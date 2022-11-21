using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiObjects.Cloudfront
{
    public interface ICloudfrontInvalidator
    {
        Task<(bool success, IEnumerable<string> failedPaths)> InvalidateAndWaitAsync(int partnerId, string[] path, WaitConfig waitConfig);
    }
}
