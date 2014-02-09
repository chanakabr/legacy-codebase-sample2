using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using Tvinci.Helpers;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class ShowInformationLoader : CustomAdapter<DataEntities.dsShows>
    {
        #region properties
        public string ShowName
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "ShowName", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "ShowName", value);

            }
        }
        #endregion properties

        public ShowInformationLoader()
        {

        }

        protected override dsShows CreateSourceResult()
        {
            dsShows SeasonShows = new dsShows();
            long ShowID = 0;

            // Fill Shows by show name table
            new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
            {
                query += "select sh.ID 'ShowID', smd.NAME 'ShowName', smd.Description, sh.IsCustomLayout ";
                query += "from Session ss left join Show sh on(ss.ShowID = sh.ID) left join ShowMetadata smd on(sh.ID = smd.ShowID) ";
                query += "where smd.NAME = '" + ShowName + "' and ";
                query += DatabaseHelper.AddCommonFields("sh.Status", "sh.is_active", eExecuteLocation.Application, false);
            }, SeasonShows.Shows).Execute();

            if (SeasonShows.Shows != null && SeasonShows.Shows.Rows.Count > 0)
            {
                ShowID = SeasonShows.Shows[0].ShowID;

                if (ShowID > 0)
                {
                    // Fill Seasons of show table
                    new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
                    {
                        query += "select ss.Session 'SeasonNumber', ss.ShowID, ss.ID 'SeasonID' ";
                        query += "from Session ss left join Show sh on(ss.ShowID = sh.ID)";
                        query += "where ss.ShowID = " + ShowID + "and ";
                        query += DatabaseHelper.AddCommonFields("ss.Status", "ss.is_active", eExecuteLocation.Application, false);
                    }, SeasonShows.Seasons).Execute();

                    foreach (dsShows.SeasonsRow row in SeasonShows.Seasons)
                    {
                        // Fill Cast tag (for season) table
                        new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
                        {
                            query += "select ss.Session 'SeasonNumber', 'Cast' 'TagType', pmd.Name 'TagDescription' ";
                            query += "from Session ss inner join SessionActors sa on(ss.ID = sa.SessionID) inner join Person pr on(sa.PersonID = pr.ID) inner join PersonMetadata pmd on(pr.ID = pmd.PersonID) ";
                            query += "where sa.SessionID = " + row.SeasonID + " and ";
                            query += DatabaseHelper.AddCommonFields("sa.Status", "sa.is_active", eExecuteLocation.Application, false);
                        }, SeasonShows.ActorsTags).Execute();

                        // Fill Directors tag (for season) table
                        new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
                        {
                            query += "select ss.Session 'SeasonNumber', 'Directors' 'TagType', pmd.Name 'TagDescription' ";
                            query += "from Session ss inner join SessionDirectors sd on(ss.ID = sd.SessionID) inner join Person pr on(sd.PersonID = pr.ID) inner join PersonMetadata pmd on(pr.ID = pmd.PersonID) ";
                            query += "where sd.SessionID = " + row.SeasonID + " and ";
                            query += DatabaseHelper.AddCommonFields("sd.Status", "sd.is_active", eExecuteLocation.Application, false);
                        }, SeasonShows.DirectorTags).Execute();

                        // Fill Genre tag (for season) table
                        new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
                        {
                            query += "select ss.Session 'SeasonNumber', 'Genre' 'TagType', lumd.Value 'TagDescription' ";
                            query += "from Session ss inner join SessionGenre sg on(ss.ID = sg.SessionID) inner join lu_Genre lug on(sg.GenreID = lug.ID) inner join lu_GenreMetadata lumd on(lug.ID = lumd.GenreID) ";
                            query += "where sg.SessionID = " + row.SeasonID + " and ";
                            query += DatabaseHelper.AddCommonFields("sg.Status", "sg.is_active", eExecuteLocation.Application, false);
                        }, SeasonShows.GenreTags).Execute();

                        // Fill Generic tags (for season) table
                        new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
                        {
                            query += "select ss.Session 'SeasonNumber', 'General' 'TagType', tv.Name 'TagDescription' ";
                            query += "from Session ss inner join SeasonTags st on(ss.ID = st.SeasonID) inner join TagView tv on (st.TagID = tv.ID) ";
                            query += "where st.SeasonID = " + row.SeasonID + " and ";
                            query += DatabaseHelper.AddCommonFields("tv.STATUS", "tv.IS_ACTIVE", eExecuteLocation.Application, false);
                        }, SeasonShows.GeneralTags).Execute();
                    }
                }
                return SeasonShows;
            }
            else
            {
                return null;
            }
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{20930399-7401-4089-95B7-47D04F8EB53F}"); }
        }
    }
}
