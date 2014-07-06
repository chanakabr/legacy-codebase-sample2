using ApiObjects.CrowdsourceItems.Base;

namespace ApiObjects.CrowdsourceItems.Implementations
{
    public class OrcaItem : BaseCrowdsourceItem
    {
        public override eItemType Type
        {
            get { return eItemType.Recommendation; }
        }
    }
}
