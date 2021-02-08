using ApiObjects.SearchObjects;

namespace ApiObjects.Base
{
    public class OrderByObject
    {
        public OrderProperty Property;
        public OrderDir Direction;
    }

    public enum OrderProperty
    {
        None,
        CreateDate,
        UpdateDate
    }
}
