using System.Linq;
using WebAPI.Exceptions;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.Catalog.SearchPriorityGroup.Validators
{
    public class KalturaSearchPriorityGroupValidator
    {
        public void ValidateToAdd(KalturaSearchPriorityGroup searchPriorityGroup, string argumentName)
        {
            if (searchPriorityGroup.Name?.Values.Any() != true)
            {
                throw new BadRequestException(ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.multilingualName");
            }

            searchPriorityGroup.Name.Validate($"{argumentName}.multilingualName");

            if (string.IsNullOrEmpty(searchPriorityGroup.Criteria?.Value))
            {
                throw new BadRequestException(ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.criteria.value");
            }
        }

        public void ValidateToUpdate(KalturaSearchPriorityGroup searchPriorityGroup, string argumentName)
        {
            if (searchPriorityGroup.Name?.Values.Any() == true)
            {
                searchPriorityGroup.Name.Validate($"{argumentName}.multilingualName");
            }
        }
    }
}