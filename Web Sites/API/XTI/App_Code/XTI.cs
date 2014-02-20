using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using TVinciShared;

public class XTI
{
    static protected XmlNamespaceManager m_nsmgr = null;
    static protected void StartNamespaceManager(ref XmlDocument theDoc)
    {
        if (m_nsmgr == null)
        {
            m_nsmgr = new XmlNamespaceManager(theDoc.NameTable);
            m_nsmgr.AddNamespace("my", "URN:NNDS:SSR:XTI:VOD:EXPORT:002");
        }
    }

    static public XmlDocument stripDocumentNamespace(XmlDocument oldDom)
    {
        // some config files have a default namespace
        // we are going to get rid of that to simplify our xpath expressions
        if (oldDom.DocumentElement.NamespaceURI.Length > 0)
        {
            oldDom.DocumentElement.SetAttribute("xmlns", "");
            // must serialize and reload the DOM
            // before this will actually take effect
            XmlDocument newDom = new XmlDocument();
            newDom.LoadXml(oldDom.OuterXml);
            return newDom;
        }
        else return oldDom;
    }

    static protected Int32 GetResponseCode(HttpStatusCode theCode)
    {
        if (theCode == HttpStatusCode.OK)
            return 200;
        if (theCode == HttpStatusCode.NotFound)
            return 404;
        return 500;

    }

    static protected string GetNodeValue(ref XmlNode theItem, string sXpath)
    {
        string sNodeVal = "";
        //XmlNode theNodeVal = theItem.SelectSingleNode(sXpath , m_nsmgr);
        XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
        if (theNodeVal != null)
            sNodeVal = theNodeVal.FirstChild.Value;
        return sNodeVal;
    }

    static public string GetLastXTIID()
    {
        string sRet = "0";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select trans_id from xti where id=1";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                sRet = selectQuery.Table("query").DefaultView[0].Row["trans_id"].ToString();
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    static public void UpdateXTIID(string sID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("xti");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("trans_id", "=", sID);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
        updateQuery += "where";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", 1);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    static protected string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
    {
        string sVal = "";
        XmlNode theRoot = theNode.SelectSingleNode(sXpath);
        if (theRoot != null)
        {
            XmlAttributeCollection theAttr = theRoot.Attributes;
            if (theAttr != null)
            {
                Int32 nCount = theAttr.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sName = theAttr[i].Name.ToLower();
                    if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                    {
                        sVal = theAttr[i].Value.ToString();
                        break;
                    }
                }
            }
        }
        return sVal;
    }

    static protected bool ProcessVodAssetItem(XmlNode theItem, string sMainLang, Int32 nGroupID, ref string sRet)
    {
        //The name of the media
        TranslatorStringHolder assetNameHolder = new TranslatorStringHolder();
        //Key Words - should be handled as one big string
        //meta7_str
        TranslatorStringHolder assetKeyWordsHolder = new TranslatorStringHolder();
        //Long description - main description
        TranslatorStringHolder assetLongDescHolder = new TranslatorStringHolder();
        //Medium description
        //media4_str
        TranslatorStringHolder assetMediumDescHolder = new TranslatorStringHolder();
        //Medium description
        //media5_str
        TranslatorStringHolder assetShortDescHolder = new TranslatorStringHolder();
        //meta1_str //media_tags_types 127 (M2M)
        TranslatorStringHolder assetSeriesNameHolder = new TranslatorStringHolder();
        //media_tags_types 93 (M2M)
        TranslatorStringHolder assetGenereHolder = new TranslatorStringHolder();
        //meta8_double
        TranslatorStringHolder assetRatingHolder = new TranslatorStringHolder();
        //Film origin language
        //media_tags_types 21 (M2M)
        TranslatorStringHolder assetLanguageHolder = new TranslatorStringHolder();
        //media_tags_types 61 (M2M)
        TranslatorStringHolder assetCountryHolder = new TranslatorStringHolder();
        //meta2_bool
        TranslatorStringHolder assetBOWHolder = new TranslatorStringHolder();
        //meta2_str
        TranslatorStringHolder assetItemTypeHolder = new TranslatorStringHolder();
        //meta7_bool + geo blocking rule
        TranslatorStringHolder assetGEOHolder = new TranslatorStringHolder();
        //meta18_str
        TranslatorStringHolder assetSongWritesHolder = new TranslatorStringHolder();
        //media_tags_types 84 (M2M)
        TranslatorStringHolder assetProjectNamesHolder = new TranslatorStringHolder();
        //media_tags_types 85 (M2M)
        TranslatorStringHolder assetSubProjectNamesHolder = new TranslatorStringHolder();
        //media_tags_types 23 (M2M)
        TranslatorStringHolder assetMoodHolder = new TranslatorStringHolder();
        //media_tags_types 65 (M2M)
        TranslatorStringHolder assetRecomendTypeHolder = new TranslatorStringHolder();
        //media_tags_types 63 (M2M)
        TranslatorStringHolder assetMovieSongsHolder = new TranslatorStringHolder();
        //media_tags_types 94 (M2M)
        TranslatorStringHolder assetSoundTrackHolder = new TranslatorStringHolder();
        //media_tags_types 95 (M2M)
        TranslatorStringHolder assetOriginalMusicHolder = new TranslatorStringHolder();
        //Film quality
        //media_tags_types 87 (M2M)
        TranslatorStringHolder assetQualityHolder = new TranslatorStringHolder();
        //Episode name
        //meta3_str
        TranslatorStringHolder assetEpisodeNameHolder = new TranslatorStringHolder();
        //Content provider name
        //meta13_str
        TranslatorStringHolder assetContentProviderNameHolder = new TranslatorStringHolder();
        //Actors
        //media_tags_types 56 (M2M)
        TranslatorStringHolder assetActorsHolder = new TranslatorStringHolder();
        //media_tags_types 24 (M2M)
        TranslatorStringHolder assetDirectorsHolder = new TranslatorStringHolder();
        //media_tags_types 53 (M2M)
        TranslatorStringHolder assetWritersHolder = new TranslatorStringHolder();
        //media_tags_types 54 (M2M)
        TranslatorStringHolder assetMusicPlayersHolder = new TranslatorStringHolder();
        //media_tags_types 55 (M2M)
        TranslatorStringHolder assetSingersHolder = new TranslatorStringHolder();
        //media_tags_types 57 (M2M)
        TranslatorStringHolder assetScriptWritersHolder = new TranslatorStringHolder();
        //media_tags_types 58 (M2M)
        TranslatorStringHolder assetMusicWritersHolder = new TranslatorStringHolder();
        //meta11_str
        TranslatorStringHolder assetOthersHolder = new TranslatorStringHolder();
        TranslatorStringHolder assetTrailerHolder = new TranslatorStringHolder();
        TranslatorStringHolder assetPosterHolder = new TranslatorStringHolder();
        TranslatorStringHolder assetThumbnailHolder = new TranslatorStringHolder();
        TranslatorStringHolder assetResPackHolder = new TranslatorStringHolder();
        TranslatorStringHolder assetBoxCovHolder = new TranslatorStringHolder();

        // The assetID
        // meta16_str

        string sAssetID = GetNodeValue(ref theItem, "assetId");
        Int32 nMediaID = GetMediaID(sAssetID, nGroupID);
        string sAction = GetNodeParameterVal(ref theItem, "VodOfferItem", "action");
        if (sAction == "delete")
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
        else
        {
            string sEndDate = GetNodeValue(ref theItem, "VodOfferItem/offerEndDateTime");


            // The provider ID - Allways Orange
            string sProviderID = GetNodeValue(ref theItem, "providerId");
            //Production Year
            //Meta4_double
            string sProductionYear = GetNodeValue(ref theItem, "exhibitionDateTime");
            //Duration
            //Will be automaticly filled
            string sDuration = GetNodeValue(ref theItem, "duration");

            //Media end_date
            string sLicenceEndDate = GetNodeValue(ref theItem, "availEndTime");
            //Media start_date
            string sStartDate = GetNodeValue(ref theItem, "availStartTime");
            //Does allow to recomend
            //meta1_bool
            string sAllowFriendRecommendationFlag = GetNodeValue(ref theItem, "criticsChoiceFlag");
            //media_tags_types 88 (M2M)
            string sWidescreenFlag = GetNodeValue(ref theItem, "widescreenFlag");
            string sLetterboxFlag = GetNodeValue(ref theItem, "letterboxFlag");
            //meta6_bool
            string sSubtitleFlag = GetNodeValue(ref theItem, "subtitleFlag");
            //media_tags_types 60 (M2M)
            string sParentalRating = GetNodeValue(ref theItem, "parentalRating");
            //media_tags_types 82 (M2M)
            string sSoundType = GetNodeValue(ref theItem, "soundType");
            //meta1_double
            string sCP = GetNodeValue(ref theItem, "VodOfferItem/billingInfo/CPAP/CP");
            //meta2_double
            string sAP = GetNodeValue(ref theItem, "VodOfferItem/billingInfo/CPAP/AP");
            //meta10_str
            string sServiceCode = GetNodeValue(ref theItem, "VodOfferItem/VodOfferSellPrice/price");
            //File size
            //meta9_double
            string sSize = GetNodeValue(ref theItem, "VodAssetDelivery/size");
            //meta17_str
            string sFecm = GetNodeValue(ref theItem, "VodOfferItem/VodOfferFecm/fecmPayLoad");
            //sFecm = HexToStr(sFecm);
            //media_tags_types 81 (M2M)
            string sPurchaseModel = GetNodeValue(ref theItem, "VodOfferItem/businessRule");
            //VodAssetDescription
            string sSeriesInfo = "";
            //meta3_num
            string sEpisodeNum = "";
            //meta5_num
            string sSeasonNum = "";
            //meta6_num
            string sEpisodesCount = "";
            //meta7_num
            string sSeasonCount = "";
            //VodAssetDescription
            //XmlNodeList theItemDescriptions = theItem.SelectNodes("VodAssetDescription", m_nsmgr);
            XmlNodeList theItemDescriptions = theItem.SelectNodes("VodAssetDescription");
            Int32 nCount = theItemDescriptions.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode descItem = theItemDescriptions[i];
                string sLang = GetNodeValue(ref descItem, "lang");
                bool bMainLang = false;
                if (sLang == sMainLang)
                    bMainLang = true;

                string sName = GetNodeValue(ref descItem, "assetName");
                string sLongDesc = GetNodeValue(ref descItem, "assetDescription");
                string sMediumDesc = GetNodeValue(ref descItem, "assetMarketingMsg");
                string sEpisodeName = GetNodeValue(ref descItem, "assetEpisodeTitle");
                string sContentProviderName = GetNodeValue(ref descItem, "assetBranding");
                if (sEpisodeNum == "")
                    sEpisodeNum = GetNodeValue(ref descItem, "assetSort");
                if (sSeriesInfo == "")
                {
                    sSeriesInfo = GetNodeValue(ref descItem, "assetSeriesInfo");
                    if (sSeriesInfo != "")
                    {
                        string[] splitedSeriesInfo = sSeriesInfo.Split('/');
                        if (splitedSeriesInfo.Length == 3)
                        {
                            sSeasonNum = splitedSeriesInfo[0];
                            sEpisodesCount = splitedSeriesInfo[1];
                            sSeasonCount = splitedSeriesInfo[2];
                        }
                    }
                }

                assetNameHolder.AddLanguageString(sLang, sName, bMainLang);
                assetLongDescHolder.AddLanguageString(sLang, sLongDesc, bMainLang);
                assetEpisodeNameHolder.AddLanguageString(sLang, sEpisodeName, bMainLang);
                assetMediumDescHolder.AddLanguageString(sLang, sMediumDesc, bMainLang);
                assetContentProviderNameHolder.AddLanguageString(sLang, sContentProviderName, bMainLang);

            }
            //XmlNodeList theItemKeyWords = theItem.SelectNodes("VodAssetKeyword", m_nsmgr);
            XmlNodeList theItemKeyWords = theItem.SelectNodes("VodAssetKeyword");
            nCount = theItemKeyWords.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode descKWItem = theItemKeyWords[i];
                string sLang = GetNodeValue(ref descKWItem, "lang");
                bool bMainLang = false;
                if (sLang == sMainLang)
                    bMainLang = true;
                string sKeyWord = GetNodeValue(ref descKWItem, "keyword");
                assetKeyWordsHolder.AddLanguageString(sLang, sKeyWord, bMainLang);
            }

            //XmlNodeList theItemCredits = theItem.SelectNodes("VodAssetCredit", m_nsmgr);
            XmlNodeList theItemCredits = theItem.SelectNodes("VodAssetCredit");
            nCount = theItemCredits.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode creditsItem = theItemCredits[i];
                string sLang = GetNodeValue(ref creditsItem, "lang");
                bool bMainLang = false;
                if (sLang == sMainLang)
                    bMainLang = true;
                string sclassificationKey = GetNodeValue(ref creditsItem, "classificationKey");
                string screditKey = GetNodeValue(ref creditsItem, "creditKey");
                string sorderNum = GetNodeValue(ref creditsItem, "orderNum");
                string spersonGivenName = GetNodeValue(ref creditsItem, "personGivenName");
                string spersonFamilyName = GetNodeValue(ref creditsItem, "personFamilyName");
                string spersonTitle = GetNodeValue(ref creditsItem, "personTitle");
                string scharacterGivenName = GetNodeValue(ref creditsItem, "characterGivenName");
                string scharacterFamilyName = GetNodeValue(ref creditsItem, "characterFamilyName");
                string scharacterTitle = GetNodeValue(ref creditsItem, "characterTitle");
                string sVal = spersonGivenName + " " + spersonFamilyName;
                if (spersonTitle != "" || scharacterGivenName != "" || scharacterFamilyName != "" || scharacterTitle != "")
                {
                    sVal += "(";
                    if (spersonTitle != "")
                        sVal += "person title: " + spersonTitle + "|";
                    if (scharacterGivenName != "" || scharacterFamilyName != "")
                        sVal += "character name: " + scharacterGivenName + " " + scharacterFamilyName + "|"; ;
                    if (scharacterTitle != "")
                        sVal += "character title: " + scharacterTitle;
                    sVal += ")";
                }
                if (sclassificationKey.Trim().ToLower().EndsWith("actor") == true)
                    assetActorsHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("director") == true)
                    assetDirectorsHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("writer") == true)
                    assetWritersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("writer") == true)
                    assetWritersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("music player") == true)
                    assetMusicPlayersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("music player") == true)
                    assetMusicPlayersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("singers") == true)
                    assetSingersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("scriptwriter") == true)
                    assetScriptWritersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("musicwriter") == true)
                    assetMusicWritersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("other") == true)
                    assetOthersHolder.AddLanguageString(sLang, sVal, screditKey, bMainLang);
            }

            //XmlNodeList theItemExtendDesc = theItem.SelectNodes("VodAssetExtendedDescription", m_nsmgr);
            XmlNodeList theItemExtendDesc = theItem.SelectNodes("VodAssetExtendedDescription");
            nCount = theItemExtendDesc.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode extendDescItem = theItemExtendDesc[i];
                string sLang = GetNodeValue(ref extendDescItem, "displayLanguage");
                string sItemNum = GetNodeValue(ref extendDescItem, "itemNumber");

                bool bMainLang = false;
                if (sLang == sMainLang)
                    bMainLang = true;
                string sassetDescriptionLabel = GetNodeValue(ref extendDescItem, "assetDescriptionLabel");
                string sassetExtendedDescription = GetNodeValue(ref extendDescItem, "assetExtendedDescription");

                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("shortdescription") == true)
                    assetShortDescHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("video item name") == true)
                    assetSeriesNameHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("category") == true)
                    assetGenereHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("rating") == true)
                    assetRatingHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("language") == true)
                    assetLanguageHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("quality") == true)
                    assetQualityHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("country") == true)
                    assetCountryHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("black and white") == true)
                    assetBOWHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("video item type") == true)
                    assetItemTypeHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("geoblock") == true)
                    assetGEOHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("song rights") == true)
                    assetSongWritesHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("project name") == true)
                    assetProjectNamesHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("sub project name") == true)
                    assetSubProjectNamesHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("moods") == true)
                    assetMoodHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("recommendation type") == true)
                    assetRecomendTypeHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);

                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("movie songs") == true)
                    assetMovieSongsHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("soundtrack") == true)
                    assetSoundTrackHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);
                if (sassetDescriptionLabel.Trim().ToLower().EndsWith("original music") == true)
                    assetOriginalMusicHolder.AddLanguageString(sLang, sassetExtendedDescription, sItemNum, bMainLang);

            }

            //XmlNodeList theItemMedia = theItem.SelectNodes("VodAssetMedia", m_nsmgr);
            XmlNodeList theItemMedia = theItem.SelectNodes("VodAssetMedia");
            nCount = theItemMedia.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode mediaItem = theItemMedia[i];
                string sLang = GetNodeValue(ref mediaItem, "mediaLanguage");
                string sMediaKey = GetNodeValue(ref mediaItem, "mediaKey");
                string sclassificationKey = GetNodeValue(ref mediaItem, "classificationKey");
                string srelationshipType = GetNodeValue(ref mediaItem, "relationshipType");
                string smediaUri = GetNodeValue(ref mediaItem, "mediaUri");

                bool bMainLang = false;
                if (sLang == sMainLang)
                    bMainLang = true;

                if (sclassificationKey.Trim().ToLower().EndsWith("trailer") == true)
                    assetTrailerHolder.AddLanguageString(sLang, smediaUri, bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("poster") == true)
                    assetPosterHolder.AddLanguageString(sLang, smediaUri, "poster", bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("thumbnail") == true)
                    assetThumbnailHolder.AddLanguageString(sLang, smediaUri, "thumbnail", bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("resource package") == true)
                    assetResPackHolder.AddLanguageString(sLang, smediaUri, "resource package", bMainLang);
                if (sclassificationKey.Trim().ToLower().EndsWith("box cover") == true)
                    assetBoxCovHolder.AddLanguageString(sLang, smediaUri, "box cover", bMainLang);

            }
            string[] sep = { ":" };
            string[] durationSplited = sDuration.Split(sep, StringSplitOptions.None);
            if (durationSplited.Length == 4)
            {
                Int32 nDurationSec = 0;
                try
                {
                    nDurationSec += int.Parse(durationSplited[0].ToString()) * 86400;
                    nDurationSec += int.Parse(durationSplited[1].ToString()) * 3600;
                    nDurationSec += int.Parse(durationSplited[2].ToString()) * 60;
                    nDurationSec += int.Parse(durationSplited[3].ToString());
                    nDurationSec *= 1000;
                    sDuration = nDurationSec.ToString();
                }
                catch
                {
                }
            }
            UpdateMediaBaseValues(nMediaID, sMainLang, nGroupID, assetNameHolder, assetLongDescHolder, assetItemTypeHolder, assetGEOHolder, sStartDate, sLicenceEndDate, sEndDate);
            IngestionUtils.TranslateMediaBaseValues(nMediaID, sMainLang, assetNameHolder, assetLongDescHolder, sAssetID, sServiceCode, "", sFecm, sDuration, assetItemTypeHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 1, assetSeriesNameHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 2, assetItemTypeHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 3, assetEpisodeNameHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 4, assetMediumDescHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 5, assetShortDescHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 6, sDuration);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 7, assetKeyWordsHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 10, sServiceCode);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 11, assetOthersHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 13, assetContentProviderNameHolder);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 16, sAssetID);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 17, sFecm);
            UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, 18, assetSongWritesHolder);

            UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 1, sAllowFriendRecommendationFlag);
            UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 2, assetBOWHolder);
            //UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 3, assetBOWHolder);
            //UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 4, assetBOWHolder);
            //UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 5, assetBOWHolder);
            UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 6, sSubtitleFlag);
            UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, 7, assetGEOHolder);

            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 1, sCP);
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 2, sAP);
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 3, sEpisodeNum);
            if (sProductionYear != "")
            {
                string[] splited = sProductionYear.Split('/');
                if (splited.Length > 0)
                    sProductionYear = splited[0];
                UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 4, sProductionYear);
            }
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 5, sSeasonNum);
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 6, sEpisodesCount);
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 7, sSeasonCount);
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 8, assetRatingHolder);
            UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, 9, sSize);

            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "93", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetGenereHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "127", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetSeriesNameHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "21", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetLanguageHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "61", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetCountryHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "84", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetProjectNamesHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "85", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetSubProjectNamesHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "23", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetMoodHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "65", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetRecomendTypeHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "63", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetMovieSongsHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "94", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetSoundTrackHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "95", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetOriginalMusicHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "87", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetQualityHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "56", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetActorsHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "24", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetDirectorsHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "53", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetWritersHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "54", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetMusicPlayersHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "55", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetSingersHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "57", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetScriptWritersHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "58", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", assetMusicWritersHolder, nGroupID, nMediaID);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "60", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", sParentalRating, nGroupID, nMediaID, true);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "82", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", sSoundType, nGroupID, nMediaID, true);
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "81", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", sPurchaseModel, nGroupID, nMediaID, true);
            string sSS = "";
            if (sWidescreenFlag.Trim().ToLower() == "1")
                sSS += "WideScreen";
            if (sLetterboxFlag.Trim().ToLower() == "1")
            {
                if (sSS != "")
                    sSS += ";";
                sSS += "Letterbox";
            }
            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", "88", "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", "heb", sSS, nGroupID, nMediaID, true);

            UpdateMediaDRMFile(nMediaID, sMainLang, nGroupID, sAssetID, sFecm, sDuration, sCP, sAP);
            Int32 nMediaTypeID = IngestionUtils.GetMediaTypeID("FLV");
            IngestionUtils.UnActivateAllMediaFilePics(nMediaTypeID, nMediaID, nGroupID, 3);
            nMediaTypeID = IngestionUtils.GetMediaTypeID("FLV1");
            IngestionUtils.UnActivateAllMediaFilePics(nMediaTypeID, nMediaID, nGroupID, 3);
            nMediaTypeID = IngestionUtils.GetMediaTypeID("FLV2");
            IngestionUtils.UnActivateAllMediaFilePics(nMediaTypeID, nMediaID, nGroupID, 3);
            UpdateMediaPromosFiles(nMediaID, sMainLang, nGroupID, assetTrailerHolder);
            UpdateMediaPics(nMediaID, sMainLang, nGroupID, assetPosterHolder, "POSTER");
            UpdateMediaPics(nMediaID, sMainLang, nGroupID, assetThumbnailHolder, "TUMBNAIL");
            UpdateMediaPics(nMediaID, sMainLang, nGroupID, assetResPackHolder, "RESOURCEPACKAGE");
            UpdateMediaPics(nMediaID, sMainLang, nGroupID, assetBoxCovHolder, "BOXCOVER");
        }

        return true;
    }

    protected Int32 GetLangID(string sCode3)
    {
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from lu_languages where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(code3)))", "=", sCode3.Trim().ToLower());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nID;
    }

    static protected void TranslateMediaBaseMetaStrValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        TranslatorStringHolder htheVal)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from lu_languages where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                string sVal = IngestionUtils.GetTransactionStringHolderValue(htheVal, "1", sLang);
                //if (sVal.Trim() != "")
                //{
                Int32 nMediaTransID = IngestionUtils.GetMediaTranslateID(nMediaID, nLangID);
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_translate");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_STR", "=", sVal);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                updateQuery += "where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaTransID);

                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                //}
            }
        }

        selectQuery.Finish();
        selectQuery = null;
    }

    static protected Int32 DoesPicExists(string sPicBaseName, Int32 nGroupID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from pics where STATUS=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", sPicBaseName);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    static protected Int32 InsertNewPic(string sName,
        string sRemarks,
        string sBaseURL,
        Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pics");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        Int32 nRet = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from pics where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    static protected void UpdateMediaDRMFile(Int32 nMediaID,
        string sMainLang,
        Int32 nGroupID,
        string sAssetID,
        string sFecm,
        string sDuration,
        string sCP,
        string sAP)
    {
        Int32 nMediaFileID = IngestionUtils.GetPicMediaFileID(9, nMediaID, nGroupID, 3, true);
        string sCDNCode = sAssetID + "," + sFecm + "," + sDuration + "," + sCP + "," + sAP;
        if (nMediaFileID != 0)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
            //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REF_ID", "=", nPicID);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_SUPLIER_ID", "=", 13);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_CODE", "=", sCDNCode);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaFileID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
    }

    /*
    static protected Int32 GetFLVActive(Int32 nMediaID , Int32 nGroupID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select max(MEDIA_TYPE_ID) as m_i from media_files where status<>2 and MEDIA_TYPE_ID in (1,11,12,13,14,15) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        if (selectQuery.Execute("query" , true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                if (selectQuery.Table("query").DefaultView[0].Row["m_i"] != DBNull.Value)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["m_i"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }
    */
    static protected void UpdateMediaPromosFiles(Int32 nMediaID,
        string sMainLang,
        Int32 nGroupID,
        TranslatorStringHolder assetTrailerHolder)
    {
        string sPromos = IngestionUtils.GetTransactionStringHolderValue(assetTrailerHolder, "1", sMainLang);
        if (sPromos == "")
            return;
        string[] splited = sPromos.Split(',');
        Int32 nCount = splited.Length;

        Int32 nMaxTypeActive = IngestionUtils.GetFLVActive(nMediaID, nGroupID);

        Int32 nMediaTypeID = 0;
        for (int i = 0; i < nCount; i++)
        {
            if (nMaxTypeActive == 0)
            {
                nMediaTypeID = IngestionUtils.GetMediaTypeID("FLV");
                nMaxTypeActive = 1;
            }
            else if (nMaxTypeActive == 1)
            {
                nMediaTypeID = IngestionUtils.GetMediaTypeID("FLV1");
                nMaxTypeActive = 11;
            }
            else if (nMaxTypeActive == 11)
            {
                nMediaTypeID = IngestionUtils.GetMediaTypeID("FLV2");
                nMaxTypeActive = 12;
            }
            else
                break;
            string sPromo = splited[i];
            if (sPromo == "")
                continue;
            IngestionUtils.UpdateMediaPromoFile(nMediaID, sMainLang, nGroupID, nMediaTypeID, sPromo, 18, 3);
        }
    }

    static protected void UpdateMediaPics(Int32 nMediaID,
        string sMainLang,
        Int32 nGroupID,
        TranslatorStringHolder assetPicHolder,
        string sPicType)
    {
        Int32 nMediaTypeID = IngestionUtils.GetMediaTypeID(sPicType);
        IngestionUtils.UnActivateAllMediaFilePics(nMediaTypeID, nMediaID, nGroupID, 3);
        string sPics = IngestionUtils.GetTransactionStringHolderValue(assetPicHolder, "1", sMainLang);
        if (sPics == "")
            return;
        string[] splited = sPics.Split(',');
        Int32 nCount = splited.Length;
        string sBasePath = HttpContext.Current.Server.MapPath("");
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);

        Int32 nInserted = 0;
        for (int i = 0; i < nCount; i++)
        {
            string sPic = splited[i];
            if (sPic == "")
                continue;
            string sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sPic);
            if (sUploadedFile == "")
                continue;
            nInserted++;
            string sUploadedFileExt = "";
            int nExtractPos = sUploadedFile.LastIndexOf(".");
            if (nExtractPos > 0)
                sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

            string sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();
            object oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID);
            string sMediaName = "XTI Pic";
            if (oMediaName != null && oMediaName != DBNull.Value)
                sMediaName = oMediaName.ToString();
            // check if sUploadedFile exists on table
            Int32 nPicID = DoesPicExists(sUploadedFile, nGroupID);
            if (nPicID == 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from media_pics_sizes where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                    TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + sPicBaseName + "_tn" + sUploadedFileExt, 90, 65, true);
                    TVinciShared.ImageUtils.RenameImage(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt);
                    TVinciShared.DBManipulator.UploadPicToGroup(sBasePath + "/pics/" + sPicBaseName + "_tn" + sUploadedFileExt, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                    TVinciShared.DBManipulator.UploadPicToGroup(sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt, sPicsFTP, sPicsFTPUN, sPicsFTPPass);

                    for (int nI = 0; nI < nCount1; nI++)
                    {
                        string sWidth = selectQuery.Table("query").DefaultView[nI].Row["WIDTH"].ToString();
                        string sHeight = selectQuery.Table("query").DefaultView[nI].Row["HEIGHT"].ToString();
                        string sEndName = sWidth + "X" + sHeight;
                        Int32 nCrop = int.Parse(selectQuery.Table("query").DefaultView[nI].Row["TO_CROP"].ToString());
                        string sTmpImage1 = sBasePath + "/pics/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                        bool bCrop = true;
                        if (nCrop == 0)
                            bCrop = false;
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), bCrop);
                        TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        nI++;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;


                nPicID = InsertNewPic(sMediaName, sUploadedFile, sPicBaseName + sUploadedFileExt, nGroupID);
                //Int32 nPicTagID = InsertNewPicTag(sMediaName, sUploadedFile, sPicBaseName, nGroupID);
            }
            IngestionUtils.M2MHandling("ID", "", "", "", "ID", "tags", "pics_tags", "pic_id", "tag_id", "true", "heb", sMediaName, nGroupID, nPicID, false);
            if (nInserted == 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", nPicID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            EnterPicMediaFile(nMediaTypeID, nMediaID, nPicID, nGroupID);
            //enter as file
        }
    }
    /*
    static protected Int32 GetMediaTypeID(string sMediaType)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetCachedSec(86400);
        selectQuery += "select id from lu_media_types where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(DESCRIPTION)))" , "=" , sMediaType.Trim().ToLower());
        if (selectQuery.Execute("query" , true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }
    */
    static protected void EnterPicMediaFile(Int32 nPicType,
        Int32 nMediaID, Int32 nPicID, Int32 nGroupID)
    {
        Int32 nMediaFileID = IngestionUtils.GetPicMediaFileID(nPicType, nMediaID, nGroupID, 3, true);
        if (nMediaFileID != 0)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REF_ID", "=", nPicID);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaFileID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
    }

    static protected void UpdateMediaBaseValues(Int32 nMediaID,
        string sMainLang,
        Int32 nGroupID,
        TranslatorStringHolder hMediaName,
        TranslatorStringHolder hMediaDesc,
        TranslatorStringHolder hItemType,
        TranslatorStringHolder hOnlyOnIsrael,
        string sStartDate,
        string sLicenceEndDate,
        string sEndDate)
    {
        string sMainMediaName = IngestionUtils.GetTransactionStringHolderValue(hMediaName, "1", sMainLang);
        string sMainMediaDesc = IngestionUtils.GetTransactionStringHolderValue(hMediaDesc, "1", sMainLang);
        string sOnlyOnIsrael = IngestionUtils.GetTransactionStringHolderValue(hOnlyOnIsrael, "1", sMainLang);
        string sItemType = IngestionUtils.GetTransactionStringHolderValue(hItemType, "1", sMainLang);
        if (sItemType == "")
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_languages where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    sItemType = IngestionUtils.GetTransactionStringHolderValue(hItemType, "1", sLang);
                    if (sItemType != "")
                        break;
                }
            }

            selectQuery.Finish();
            selectQuery = null;
        }
        Int32 nMediaTypeID = IngestionUtils.GetMediaTypeID(sItemType, nGroupID);
        bool bOnlyOnIsrael = false;
        if (sOnlyOnIsrael == "0")
            bOnlyOnIsrael = false;
        else
            bOnlyOnIsrael = true;

        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sMainMediaName);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sMainMediaDesc);
        if (bOnlyOnIsrael == true)
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BLOCK_TEMPLATE_ID", "=", 16);
        else
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BLOCK_TEMPLATE_ID", "=", 0);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nMediaTypeID);
        if (sStartDate != "")
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", TVinciShared.DateUtils.GetDateFromStrUTF(sStartDate));
        DateTime dEndDate = new DateTime();
        if (sLicenceEndDate != "")
            dEndDate = TVinciShared.DateUtils.GetDateFromStrUTF(sLicenceEndDate);
        if (sEndDate != "")
        {
            DateTime dTemp = TVinciShared.DateUtils.GetDateFromStrUTF(sEndDate);
            if (dTemp > dEndDate)
                dEndDate = dTemp;
        }
        if (dEndDate != new DateTime())
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
        }
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);

        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    static protected void UpdateMediaBaseMetaStrValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        TranslatorStringHolder htheVal)
    {
        string sVal = IngestionUtils.GetTransactionStringHolderValue(htheVal, "1", sMainLang);
        UpdateMediaBaseMetaStrValues(nMediaID, sMainLang, nMetaID, sVal);
        TranslateMediaBaseMetaStrValues(nMediaID, sMainLang, nMetaID, htheVal);
    }

    static protected void UpdateMediaBaseMetaDoubleValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        string sVal)
    {
        if (sVal != "")
        {
            try
            {
                UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, nMetaID, double.Parse(sVal));
            }
            catch
            {
            }
        }
    }

    static protected void UpdateMediaBaseMetaDoubleValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        TranslatorStringHolder htheVal)
    {
        string sVal = IngestionUtils.GetTransactionStringHolderValue(htheVal, "1", sMainLang);
        UpdateMediaBaseMetaDoubleValues(nMediaID, sMainLang, nMetaID, sVal);
    }

    static protected void UpdateMediaBaseMetaDoubleValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        double sVal)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("meta" + nMetaID.ToString() + "_double", "=", sVal);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    static protected void UpdateMediaBaseMetaBoolValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        string sVal)
    {
        if (sVal != "")
        {
            try
            {
                if (sVal.Trim() == "1" || sVal.Trim() == "0")
                    UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, nMetaID, double.Parse(sVal));
            }
            catch
            {
            }
        }
    }

    static protected void UpdateMediaBaseMetaBoolValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        TranslatorStringHolder htheVal)
    {
        string sVal = IngestionUtils.GetTransactionStringHolderValue(htheVal, "1", sMainLang);
        UpdateMediaBaseMetaBoolValues(nMediaID, sMainLang, nMetaID, sVal);
    }

    static protected void UpdateMediaBaseMetaBoolValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        double sVal)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("meta" + nMetaID.ToString() + "_bool", "=", sVal);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    static protected void UpdateMediaBaseMetaStrValues(Int32 nMediaID,
        string sMainLang,
        Int32 nMetaID,
        string sVal)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("meta" + nMetaID.ToString() + "_str", "=", sVal);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }


    static protected Int32 GetMediaID(string sAssetID, Int32 nGroupID)
    {
        Int32 nMediaID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetCachedSec(0);
        selectQuery += "select id from media where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("META16_STR", "=", sAssetID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;

        if (nMediaID == 0)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by the XTI Service");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("META16_STR", "=", sAssetID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            return GetMediaID(sAssetID, nGroupID);
        }
        return nMediaID;
    }

    public static string ByteArrayToStr(byte[] arr)
    {
        return System.Text.UTF8Encoding.UTF8.GetString(arr);
    }

    public static string HexToStr(string sHex)
    {
        byte[] b = HexToByteArray(sHex);
        string sUTF8 = ByteArrayToStr(b);
        return sUTF8;
    }

    public static byte[] HexToByteArray(String HexString)
    {
        int NumberChars = HexString.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
        }
        return bytes;
    }

    static public string GetVodAssetList(string sTransID, string sMainLang, Int32 nGroupID)
    {
        //Logger.Logger.Log("XML From XTI", "start:0", "XTI_Export");
        string sRet = "<response>";
        string sMainContent = "<BasicExportRequest xmlns=\"URN:NNDS:SSR:XTI:VOD:EXPORT:002\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"URN:NNDS:SSR:XTI:VOD:EXPORT:002 vodexport_request.xsd\" ";
        if (sTransID != "0" && sTransID != "")
            sMainContent += " transactionIdentifier=\"" + sTransID + "\"";
        sMainContent += ">\r\n";
        sMainContent += "<VodAssetList>\r\n";
        sMainContent += "<catalogueKey>GibraltarVod</catalogueKey>\r\n";
        sMainContent += "</VodAssetList>\r\n";
        sMainContent += "</BasicExportRequest>";
        Int32 nStatus = 0;
        bool bDebug = false;
        string sXml = "";
        //Logger.Logger.Log("XML From XTI", "start:1", "XTI_Export");
        //if (bDebug == false)
        //sXml = PostHttpReq("http://172.21.113.1:8080/XTI/upload", sMainContent, ref nStatus);
        //sXml = PostHttpReq("http://MWGIB.orange.co.il:8090/XTI/upload", sMainContent, ref nStatus);
        sXml = PostHttpReq("http://MWGIB.orange.co.il:8090", sMainContent, ref nStatus);
        //Logger.Logger.Log("XML From XTI", "start:2", "XTI_Export");
        //sXml = PostHttpReq("http://192.118.32.103:8080/XTI/upload", sMainContent, ref nStatus);
        sXml = sXml.Replace("&lt;![CDATA[&lt;CPAP&gt;&lt;CP&gt;", "<CPAP><CP>").Replace("&lt;/CP&gt;&lt;AP&gt;", "</CP><AP>").Replace("&lt;/AP&gt;&lt;/CPAP&gt;]]&gt;", "</AP></CPAP>");
        Logger.Logger.Log("XML From XTI", sXml, "XTI_Export");
        XmlDocument theDoc = new XmlDocument();
        try
        {
            if (bDebug == false)
                theDoc.LoadXml(sXml);
            else
                theDoc.Load("E:/temp/nds2.xml");
            theDoc = stripDocumentNamespace(theDoc);
            //StartNamespaceManager(ref theDoc);
            //XmlNodeList theItems = theDoc.SelectNodes("/BasicExport/VodAssetList/VodAssets/VodAssetItem", m_nsmgr);
            string transactionIdentifier = "";
            XmlNode theRoot = theDoc.SelectSingleNode("/BasicExport");
            if (theRoot != null)
            {
                XmlAttributeCollection theAttr = theRoot.Attributes;
                if (theAttr != null)
                {
                    Int32 nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName == "transactionidentifier")
                        {
                            transactionIdentifier = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }
            sRet += "<transactionidentifier value=\"" + transactionIdentifier + "\">";
            XmlNodeList theItems = theDoc.SelectNodes("/BasicExport/VodAssetList/VodAssets/VodAssetItem");
            //XmlNodeList theItems = theDoc.SelectSingleNode("//VodAssetList");
            Int32 nCount1 = theItems.Count;
            for (int i = 0; i < nCount1; i++)
            {
                bool bProcess = ProcessVodAssetItem(theItems[i], sMainLang, nGroupID, ref sRet);
                if (bProcess == false)
                    break;
            }
            if (transactionIdentifier != "")
                UpdateXTIID(transactionIdentifier);
            sRet += "</transactionidentifier>";
            return sRet;
        }
        catch (Exception ex)
        {
            sRet = "<response><error>" + ex.Message + "</error>";
        }
        sRet += "</response>";
        return sRet;
    }

    static private string PostHttpReq(string sUrl, string sToSend, ref Int32 nStatus)
    {
        long length = 0;
        string boundary = "----------------------------" +
        DateTime.Now.Ticks.ToString("x");

        HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create(sUrl);
        Encoding enc = new UTF8Encoding(false);
        httpWebRequest2.ContentType = "multipart/form-data; boundary=" + boundary;
        httpWebRequest2.Method = "POST";
        httpWebRequest2.KeepAlive = true;

        httpWebRequest2.Credentials = System.Net.CredentialCache.DefaultCredentials;
        Stream memStream = new System.IO.MemoryStream();
        byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

        memStream.Write(boundarybytes, 0, boundarybytes.Length);
        length += boundarybytes.Length;

        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";

        string header = string.Format(headerTemplate, "xmlfile", "file.xml");
        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
        memStream.Write(headerbytes, 0, headerbytes.Length);
        length += headerbytes.Length;

        byte[] byteToSend = Encoding.UTF8.GetBytes(sToSend);
        memStream.Write(byteToSend, 0, byteToSend.Length);
        length += byteToSend.Length;

        memStream.Write(boundarybytes, 0, boundarybytes.Length);
        length += boundarybytes.Length;

        httpWebRequest2.ContentLength = memStream.Length;

        Stream requestStream = httpWebRequest2.GetRequestStream();
        memStream.Position = 0;
        byte[] tempBuffer = new byte[memStream.Length];
        memStream.Read(tempBuffer, 0, tempBuffer.Length);
        memStream.Close();
        requestStream.Write(tempBuffer, 0, tempBuffer.Length);
        requestStream.Close();

        Int32 nStatusCode = -1;
        //Handle the response.
        try
        {
            HttpWebResponse oWebResponse = (HttpWebResponse)httpWebRequest2.GetResponse();
            HttpStatusCode sCode = oWebResponse.StatusCode;
            nStatusCode = GetResponseCode(sCode);
            Stream receiveStream = oWebResponse.GetResponseStream();

            StreamReader sr = new StreamReader(receiveStream, enc);
            string resultString = sr.ReadToEnd();

            sr.Close();
            httpWebRequest2 = null;
            oWebResponse = null;
            nStatus = nStatusCode;
            return resultString;
        }
        catch
        {
            nStatusCode = 404;
            return "";
        }
        /*
        WebResponse webResponse2 = httpWebRequest2.GetResponse();

        Stream stream2 = webResponse2.GetResponseStream();
        StreamReader reader2 = new StreamReader(stream2);


        string s = reader2.ReadToEnd();

        webResponse2.Close();
        httpWebRequest2 = null;
        webResponse2 = null;
        */
    }

}

