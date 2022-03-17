using System.Linq;
using WebAPI.Exceptions;
using WebAPI.ObjectsConvertor.Extensions;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.Catalog.SearchPriorityGroup.Validators
{
    public class KalturaSearchPriorityGroupOrderedListValidator
    {
        private const int MAX_PRIORITY_GROUP_COUNT = 10;

        public void Validate(KalturaSearchPriorityGroupOrderedIdsSet orderedList, string argumentName)
        {
            var priorityGroupIdsArgument = $"{argumentName}.priorityGroupIds";

            var ids = orderedList.GetPriorityGroupIds().ToArray();

            if (ids.Length > MAX_PRIORITY_GROUP_COUNT)
            {
                throw new BadRequestException(ARGUMENT_MAX_ITEMS_CROSSED, argumentName, MAX_PRIORITY_GROUP_COUNT);
            }

            if (ids.Length != ids.Distinct().Count())
            {
                throw new BadRequestException(ARGUMENTS_VALUES_DUPLICATED, priorityGroupIdsArgument);
            }
        }
    }
}