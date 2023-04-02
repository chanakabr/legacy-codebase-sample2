using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiObjects;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog.Lineup;

namespace WebAPI.Validation
{
    public class LineupRequestValidator : ILineupRequestValidator
    {
        private const int MIN_PAGE_INDEX = 1;
        private const int DEFAULT_PAGE_SIZE = 500;

        private static readonly Lazy<LineupRequestValidator> ValidatorLazy =
            new Lazy<LineupRequestValidator>(() => new LineupRequestValidator(), LazyThreadSafetyMode.PublicationOnly);

        public int MinPageIndex => MIN_PAGE_INDEX;
        public int DefaultPageSize => DEFAULT_PAGE_SIZE;

        public IEnumerable<int> AllowedPageSizes => new[] { 100, 200, 800, 1200, 1600 };
        public static ILineupRequestValidator Instance => ValidatorLazy.Value;

        public bool ValidatePageIndex(int pageIndex) => pageIndex >= MIN_PAGE_INDEX;

        public bool ValidatePageSize(int pageSize) => AllowedPageSizes.Contains(pageSize);

        public void ValidateRequestFilter(KalturaLineupRegionalChannelFilter filter)
        {
            if (filter.LcnGreaterThanOrEqual > filter.LcnLessThanOrEqual)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, nameof(filter.LcnLessThanOrEqual), nameof(filter.LcnGreaterThanOrEqual));
            }
        }
    }
}