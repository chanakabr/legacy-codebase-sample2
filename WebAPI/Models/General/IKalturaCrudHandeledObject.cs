using ApiLogic.Base;
using ApiObjects.Base;

namespace WebAPI.Models.General
{
    public interface IKalturaCrudHandeledObject<T> where T : ICrudHandeledObject
    {
        ICrudHandler<T> GetHandler();
        void ValidateForAdd();
        void ValidateForUpdate();
    }
}