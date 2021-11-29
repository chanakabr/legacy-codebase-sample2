using WebAPI.Exceptions;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.Catalog.SearchPriorityGroup.Validators
{
    public class KalturaSearchPriorityGroupFilterValidator
    {
        public void Validate(KalturaSearchPriorityGroupFilter filter, string argumentName)
        {
            if (filter.ActiveOnly && filter.IdEqual.HasValue)
            {
                throw new BadRequestException(ARGUMENTS_CONFLICTS_EACH_OTHER, $"{argumentName}.activeOnlyEqual", $"{argumentName}.idEqual");
            }
        }
    }
}