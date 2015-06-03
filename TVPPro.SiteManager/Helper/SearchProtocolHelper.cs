using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
//using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using TVPPro.SiteManager.DataEntities;
//using Tvinci.Projects.TVP.Core.Configuration.Technical;

namespace TVPPro.SiteManager.Helper
{
    public static class SearchProtocolHelper
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //*******************************************
        // NOTICE!!!!!!
        // Must starts/ends with ';' and must be lowercase!!
        private const string TagsToHandle = ";mood;music players;genre;music players;singers;actors;quality;parental control;country;movie songs;project name;sub project name;script writer;director;";
        //*******************************************

        private const string TagCollectionItemSplitter = "|";
        private static void handleMetadata(dsTVMMediaInfo.MediaRow row, media media)
        {
            if (media.tags_collections != null)
            {
                foreach (tags_collectionstag_type tagType in media.tags_collections)
                {
                    if (TagsToHandle.IndexOf(string.Format(";{0};", tagType.name.ToLower())) != -1)
                    {
                        StringBuilder sb = new StringBuilder();

                        if (tagType.Count == 0)
                        {
                            continue;
                        }

                        foreach (tag tag in tagType.tagCollection)
                        {
                            if (sb.Length != 0)
                            {
                                sb.Append(TagCollectionItemSplitter);
                            }

                            sb.Append(tag.name);
                        }

                        switch (tagType.name.ToLower())
                        {
                            case "genre":
                                row.Genre = sb.ToString();
                                break;
                            case "mood":
                                row.Moods = sb.ToString();
                                break;
                            case "actors":
                                row.Actors = sb.ToString();
                                break;
                            case "music players":
                                row.MusicPlayers = sb.ToString();
                                break;
                            case "country":
                                row.Country = sb.ToString();
                                break;
                            case "director":
                                row.Director = sb.ToString();
                                break;
                            case "script writer":
                                row.Screenwriter = sb.ToString();
                                break;
                            case "quality":
                                row.Quality = sb.ToString();
                                break;
                            case "singers":
                                row.Singers = sb.ToString();
                                break;
                            case "parental control":
                                row.ParentalControl = sb.ToString();
                                break;
                            case "project name":
                                row.ProjectName = sb.ToString();
                                break;
                            case "movie songs":
                                row.MovieSongs = sb.ToString();
                                break;
                            case "sub project name":
                                row.SubProjectName = sb.ToString();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public static dsTVMMediaInfo.MediaRow ParseMediaInfoElement(media media, bool shouldHandleTags)
        {
            dsTVMMediaInfo.MediaRow row = new dsTVMMediaInfo().Media.NewMediaRow();
            ParseMediaInfoElement(row, media, shouldHandleTags);

            return row;
        }


        public static void ParseMediaInfoElement(dsTVMMediaInfo.MediaRow row, media media, bool shouldHandleTags)
        {
            if (!string.IsNullOrEmpty(media.id))
            {
                row.ID = media.id;

                row.Type = media.type.value;
                row.Name = media.title;

                if (!string.IsNullOrEmpty(media.rating.avg))
                {
                    double temp;
                    if (Double.TryParse(media.rating.avg, out temp))
                    {
                        row.Rate = temp;
                    }
                }

                row.Views = media.views.count;
                if (!string.IsNullOrEmpty(media.date))
                {
                    DateTime addedDate;
                    if (DateTime.TryParseExact(media.date, @"dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out addedDate))
                    {
                        row.AddedDate = addedDate;
                    }
                    else
                    {
                        logger.ErrorFormat("Invalid date returned for item '{0}' from TVM.Returned Value = '{1}'", media.id, media.date);
                    }
                }
                else
                {
                    row.AddedDate = new DateTime(1980, 8, 11);
                    logger.WarnFormat("No added date returned for item '{0}' from TVM. Dummy value is set (11-8-1980)", media.id);
                }
                row.ImageLink = media.pic_size1;

                row.ShowName = media.META1_STR_NAME.value;
                row.EpisodeName = media.META3_STR_NAME.value;
                row.Description = media.META4_STR_NAME.value;
                row.DescriptionShort = media.META5_STR_NAME.value;
                //row.Length = media.META6_STR_NAME.value;                        
                row.Keyword1 = media.META7_STR_NAME.value;
                row.Keyword2 = media.META8_STR_NAME.value;
                row.Keyword3 = media.META9_STR_NAME.value;
                row.ServiceCode = media.META10_STR_NAME.value;

                //switch (TechnicalConfiguration.Config.TemporaryDefinitions.AssetIDType)
                //{
                //    case AssetIDType.Meta16:
                //        row.AssetID = media.META16_STR_NAME.value;
                //        break;
                //    case AssetIDType.MediaID:
                //        row.AssetID = media.id;
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //        break;
                //}

                row.FECM = media.META17_STR_NAME.value;

                row.CP = media.META1_DOUBLE_NAME.value;
                row.AP = media.META2_DOUBLE_NAME.value;
                row.Episode = media.META3_DOUBLE_NAME.value;
                if (!string.IsNullOrEmpty(media.META4_DOUBLE_NAME.value))
                {
                    int temp;
                    if (Int32.TryParse(media.META4_DOUBLE_NAME.value, out temp))
                    {
                        row.PublishYear = temp;
                    }
                }
                row.Session = media.META5_DOUBLE_NAME.value;
                row.PlayMethod = media.META10_DOUBLE_NAME.value;

                if (shouldHandleTags)
                {
                    handleMetadata(row, media);
                }
            }
        }
    }
}
