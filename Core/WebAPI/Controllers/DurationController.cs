using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    [Service("duration")]
    class DurationController : IKalturaController
    {
        /// <summary>
        /// Get the list of optinal Duration codes
        /// </summary>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaDurationListResponse List()
        {
            KalturaDurationListResponse result = new KalturaDurationListResponse();

            // years and months have specific code for duration any other time represented in minutes so no need to get it from list
            List<Duration> durations = Duration.GetDurationsByUnit(DurationUnit.Years);
            durations.AddRange(Duration.GetDurationsByUnit(DurationUnit.Months));

            result.Objects = AutoMapper.Mapper.Map<List<KalturaDuration>>(durations);
            result.TotalCount = result.Objects.Count;

            return result;
        }
    }
}