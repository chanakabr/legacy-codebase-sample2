using BuzzFeeder.Implementation.Channels;
using BuzzFeeder.Implementation.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder
{
    public class BuzzFeeder : ScheduledTasks.BaseTask
    {
        private int m_nGroupID;
        private DateTime m_dtCurTime;
        private TimeSpan m_tsInterval;

        private BuzzWrapper m_oBuzzWrapper;
        private List<string> m_lAssetTypes;
        private string m_sSeriesTagType;
        private string[] m_sSeriesMediaTypeId;

        public BuzzFeeder(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            m_oBuzzWrapper = null;
            m_dtCurTime = DateTime.UtcNow;
            m_lAssetTypes = new List<string>();
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new BuzzFeeder(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            bool bRes = false;
            InitParamters();

            if (m_oBuzzWrapper != null)
            {
                m_oBuzzWrapper.CalculateBuzz();
                bRes = true;
            }

            return bRes;
        }

        protected bool InitParamters()
        {
            bool bRes = false;

            Logger.Logger.Log("Info", "Start Init Params.", "BuzzFeeder");

            if (string.IsNullOrEmpty(m_sParameters))
            {
                Logger.Logger.Log("Error", "parameters passed to feeder are null or empty", "BuzzFeeder");
                return bRes;
            }

            Logger.Logger.Log("Info", string.Concat("Input parameters received=", m_sParameters), "BuzzFeeder");

            string sBuzzerType; // series/channels

            string[] splitString = m_sParameters.Split('|');

            if (splitString.Length < 5)
            {
                Logger.Logger.Log("Error", "Number of parameters passed must be at least 5", "BuzzFeeder");
            }

            int.TryParse(splitString[0], out m_nGroupID);
            sBuzzerType = splitString[1];
            
            int nInterval;
            if (int.TryParse(splitString[2], out nInterval))
            {
                m_tsInterval = new TimeSpan(0, nInterval, 0);
                m_lAssetTypes.AddRange(splitString[3].Split(';'));

                if (m_lAssetTypes.Count > 0)
                {
                    bool bSuccess = true;
                    int nStartIndex = 4;
                    if (sBuzzerType == "series")
                    {
                        nStartIndex = 6;

                        if (splitString.Length < 7)
                        {
                            Logger.Logger.Log("Error", "series is missing series tag type / series media type id", "BuzzFeeder");
                            bSuccess = false;
                        }
                        else
                        {
                            m_sSeriesTagType = splitString[4];
                            m_sSeriesMediaTypeId = splitString[5].Split('#');

                            if (m_sSeriesMediaTypeId.Length == 0 || m_sSeriesTagType.Length == 0)
                            {
                                bSuccess = false;
                                Logger.Logger.Log("Error", "series series tag type/series media type id are empty", "BuzzFeeder");
                            }

                        }
                    }

                    m_oBuzzWrapper = new BuzzWrapper(m_nGroupID, m_lAssetTypes);
                    if (bSuccess)
                    {
                        for (int i = nStartIndex; i < splitString.Length; i++)
                        {
                            AddBuzzActivityFromParam(sBuzzerType, splitString[i]);
                        }
                    }
                }
                else
                {
                    Logger.Logger.Log("Error", string.Format("Did not receive asset types. input={0}", splitString[3]), "BuzzFeeder");
                }
            }
            else
            {
                Logger.Logger.Log("Error", string.Format("Could not parse time interval {0}", splitString[2]), "BuzzFeeder");
            }

            return bRes;
        }

        private void AddBuzzActivityFromParam(string sBuzzType, string sParams)
        {

            if (string.IsNullOrEmpty(sParams))
            {
                return;
            }

            string[] splitParams = sParams.Split(';');

            if (splitParams.Length < 4)
            {
                Logger.Logger.Log("Error", string.Format("Parsing activity type has failed due to invalid number of arguments {0}; input={1}", splitParams.Length, sParams), "BuzzFeeder");
                return;
            }

            try
            {
                eBuzzActivityTypes eBuzzActivityType = (eBuzzActivityTypes)Enum.Parse(typeof(eBuzzActivityTypes), splitParams[0].ToUpper());

                if (Enum.IsDefined(typeof(eBuzzActivityTypes), eBuzzActivityType))
                {
                    int nWeight;
                    int.TryParse(splitParams[1], out nWeight);

                    string[] activityFormulaWeights = splitParams[2].Split('#');
                    string[] actions = splitParams[3].Split('#');

                    BaseBuzzImpl buzzImpl = CreateBuzzInstance(sBuzzType, eBuzzActivityType, nWeight, actions, activityFormulaWeights);

                    if (buzzImpl != null)
                        m_oBuzzWrapper.AddActivity(eBuzzActivityType,buzzImpl);
                    
                }
                else
                {
                    Logger.Logger.Log("Error", string.Format("Unable to parse activity type. input={0}", splitParams[0]), "BuzzFeeder");
                }
            }
            catch (Exception ex)
            {
                m_oBuzzWrapper = null;
                Logger.Logger.Log("Error", string.Format("Caught exception when creating buzz activity. ex={0};stack={1}", ex.Message, ex.StackTrace), "BuzzFeeder");
            }
        }

        private BaseBuzzImpl CreateBuzzInstance(string sBuzzType, eBuzzActivityTypes eActivityType, int nWeight, string[] lActions, string[] lFormulaWeights)
        {
            BaseBuzzImpl oRes = null;

            if (nWeight <= 0 || lActions == null || lActions.Length == 0 || lFormulaWeights == null || lFormulaWeights.Length != 4)
            {
                Logger.Logger.Log("Error", "Recevied actions are empty or formula weight do not contain weights for all 4", "BuzzFeeder");
                return oRes;
            }

            List<int> lWeights = new List<int>();

            foreach (string sWeight in lFormulaWeights)
            {
                int nVal;
                int.TryParse(sWeight, out nVal);

                lWeights.Add(nVal);
            }

            if (sBuzzType == "series")
            {
                switch (eActivityType)
                {
                    case eBuzzActivityTypes.COMMENTS:
                        oRes = new TvinciSeriesCommentsBuzzImpl(m_nGroupID, m_sSeriesTagType, m_sSeriesMediaTypeId, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                    case eBuzzActivityTypes.FAVORITES:
                        oRes = new TvinciSeriesFavoritesBuzzImpl(m_nGroupID, m_sSeriesTagType, m_sSeriesMediaTypeId, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                    case eBuzzActivityTypes.LIKES:
                        oRes = new TvinciSeriesLikesBuzzImpl(m_nGroupID, m_sSeriesTagType, m_sSeriesMediaTypeId, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                    case eBuzzActivityTypes.VIEWS:
                        oRes = new TvinciSeriesViewsBuzzImpl(m_nGroupID, m_sSeriesTagType, m_sSeriesMediaTypeId, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                }
            }
            else if (sBuzzType == "channel")
            {
                switch (eActivityType)
                {
                    case eBuzzActivityTypes.COMMENTS:
                        oRes = new TvinciChannelCommentsBuzzImpl(m_nGroupID, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                    case eBuzzActivityTypes.FAVORITES:
                        oRes = new TvinciChannelFavoritesBuzzImpl(m_nGroupID, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                    case eBuzzActivityTypes.LIKES:
                        oRes = new TvinciChannelLikesBuzzImpl(m_nGroupID, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                    case eBuzzActivityTypes.VIEWS:
                        oRes = new TvinciChannelViewsBuzzImpl(m_nGroupID, m_dtCurTime, m_tsInterval, nWeight, lActions.ToList(), m_lAssetTypes.ToList(), lWeights);
                        break;
                }
            }

            return oRes;
        }

    }
}
