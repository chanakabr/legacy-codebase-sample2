using System.Threading.Tasks;

namespace ApiObjects
{
    public interface IUserWatchHistoryManager
    {
        Task CleanByRetention(long userId);
    }
}