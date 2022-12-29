using ApiObjects.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApiObjects
{
    public class CampaignFilter : ICrudFilter
    {
        public CampaignOrderBy? OrderBy { get; set; }

        public List<T> ApplyOrderBy<T>(List<T> campaigns) where T : Campaign, new()
        {
            if (this.OrderBy.HasValue && campaigns?.Count > 0)
            {
                switch (this.OrderBy.Value)
                {
                    case CampaignOrderBy.StartDateDesc:
                        campaigns = campaigns.OrderByDescending(camp => camp.StartDate).ToList();
                        break;
                    case CampaignOrderBy.StartDateAsc:
                        campaigns = campaigns.OrderBy(camp => camp.StartDate).ToList();
                        break;
                    case CampaignOrderBy.UpdateDateDesc:
                        campaigns = campaigns.OrderByDescending(camp => camp.UpdateDate).ToList();
                        break;
                    case CampaignOrderBy.UpdateDateAsc:
                        campaigns = campaigns.OrderBy(camp => camp.UpdateDate).ToList();
                        break;
                    case CampaignOrderBy.EndDateDesc:
                        campaigns = campaigns.OrderByDescending(camp => camp.EndDate).ToList();
                        break;
                    case CampaignOrderBy.EndDateAsc:
                        campaigns = campaigns.OrderBy(camp => camp.EndDate).ToList();
                        break;
                    default:
                        throw new NotImplementedException(String.Concat("Unsupported OrderBy type: ", this.OrderBy.ToString()));
                }
            }

            return campaigns;
        }
    }

    public class CampaignIdInFilter : CampaignFilter
    {
        public List<long> IdIn { get; set; }
        public bool IsAllowedToViewInactiveCampaigns { get; set; }
    }

    public class CampaignSearchFilter : CampaignFilter
    {
        public long? StartDateGreaterThanOrEqual { get; set; }
        public long? EndDateLessThanOrEqual { get; set; }
        public CampaignState? StateEqual { get; set; }
        public bool? HasPromotion { get; set; }
        public bool IsActiveNow { get; set; }
        public string NameEqual { get; set; }
        public string NameContains { get; set; }
        public List<CampaignState> StateIn { get; set; }

        public IEnumerable<CampaignDB> Apply(IEnumerable<CampaignDB> campaignsDB)
        {
            if (campaignsDB?.Count() == 0)
            {
                return null;
            }

            if (this.StartDateGreaterThanOrEqual.HasValue)
            {
                campaignsDB = campaignsDB.Where(x => x.StartDate >= this.StartDateGreaterThanOrEqual.Value);
            }

            if (this.EndDateLessThanOrEqual.HasValue)
            {
                campaignsDB = campaignsDB.Where(x => x.EndDate <= this.EndDateLessThanOrEqual.Value);
            }

            if (this.HasPromotion.HasValue)
            {
                campaignsDB = campaignsDB.Where(x => x.HasPromotion == this.HasPromotion.Value);
            }

            if (this.IsActiveNow)
            {
                campaignsDB = FilterByState(campaignsDB);
            }

            return campaignsDB;
        }

        public List<T> Apply<T>(List<T> campaigns) where T : Campaign, new()
        {
            if (!string.IsNullOrEmpty(this.NameEqual))
            {
                campaigns = campaigns.FindAll(x => x.Name == this.NameEqual);
            }

            if (!string.IsNullOrEmpty(this.NameContains))
            {
                campaigns = campaigns.FindAll(x => x.Name.Contains(this.NameContains));
            }

            if (!this.IsActiveNow)
            {
                return FilterByState(campaigns).ToList();
            }

            return campaigns;
        }

        private IEnumerable<T> FilterByState<T>(IEnumerable<T> campaigns) where T : CampaignDB, new()
        {
            if (this.StateEqual.HasValue)
            {
                var filteredCampaignsByStateEqual = ApplyState(campaigns, this.StateEqual.Value);
                return filteredCampaignsByStateEqual;
            }

            if (!(this.StateIn?.Any() ?? false))
            {
                return campaigns;
            }

            var filteredCampaignsByStateIn = new List<T>();
            foreach (var stateEqual in this.StateIn)
            {
                filteredCampaignsByStateIn.AddRange(ApplyState(campaigns, stateEqual));
            }
            return filteredCampaignsByStateIn;
        }

        private IEnumerable<T> ApplyState<T>(IEnumerable<T> campaigns, CampaignState stateEqual) where T : CampaignDB, new()
        {
            var utcNow = GetUtcUnixTimestampNow();
            switch (stateEqual)
            {
                case CampaignState.INACTIVE:
                    var inactiveCampaigns = campaigns.Where(x => x.State == stateEqual);
                    return inactiveCampaigns;
                case CampaignState.ACTIVE:
                    var activeCampaigns = campaigns.Where(x => x.EndDate >= utcNow && x.State == stateEqual);
                    if (this.IsActiveNow)
                    {
                        activeCampaigns = activeCampaigns.Where(x => x.StartDate <= utcNow);
                    }
                    return activeCampaigns;
                case CampaignState.ARCHIVE:
                    var archiveCampaigns = campaigns.Where(x => (x.State == stateEqual) || (x.EndDate < utcNow && x.State == CampaignState.ACTIVE));
                    return archiveCampaigns;
                default:
                    throw new NotImplementedException($"ApplyOnState was not implemented for state {stateEqual}");
            }
        }

        private long GetUtcUnixTimestampNow()
        {
            TimeSpan ts = DateTime.UtcNow - GetTruncDateTimeUtc();
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        private DateTime GetTruncDateTimeUtc()
        {
            DateTime truncDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return truncDateTimeUtc;
        }
    }

    public class TriggerCampaignFilter : CampaignSearchFilter
    {
        public ApiService? Service { get; set; }
        public ApiAction? Action { get; set; }
    }

    public class BatchCampaignFilter : CampaignSearchFilter
    {
    }

    public class CampaignSegmentFilter : CampaignSearchFilter
    {
        public long SegmentIdEqual { get; set; }
    }

    public enum CampaignOrderBy
    {
        StartDateDesc,
        StartDateAsc,
        UpdateDateDesc,
        UpdateDateAsc,
        EndDateDesc,
        EndDateAsc
    }
}