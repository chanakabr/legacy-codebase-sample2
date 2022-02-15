using ApiObjects;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EpgBL
{
    public class EpgManager
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string EPG_SEQUENCE_DOCUMENT = "epg_sequence_document";
        private int _GroupId;

        public EpgManager(int groupId)
        {
            _GroupId = groupId;
        }

        public void SetEpgIds(IList<EpgCB> programsToAdd)
        {
            if (programsToAdd != null && programsToAdd.Count > 0)
            {
                var couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.EPG);
                // capture the ids for the entire range to update instead of calling this for every program
                var lastNewEpgId = couchbaseManager.Increment(EPG_SEQUENCE_DOCUMENT, (ulong)programsToAdd.Count + 1);
                var firstNewEpgId = lastNewEpgId - (ulong)programsToAdd.Count;

                firstNewEpgId += (ulong)ApplicationConfiguration.Current.EpgInitialId.Value;
                // set the new programs with new IDs from sequence document in couchbase
                foreach (var program in programsToAdd)
                {
                    program.EpgID = firstNewEpgId++;
                }
            }
        }
    }
}
