using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using Tvinci.Core.DAL;

namespace CouchbaseMediaMarksFeeder
{
    /*
     * 1. This class is thread-safe (unless you temper with it)
     */
    internal class ChunkManager : IDisposable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private int index;
        private bool isInitialized;
        private int total;
        private int bulkSize;
        private DateTime fromDate;
        private DateTime toDate;
        private int groupID;
        private readonly object locker;


        internal ChunkManager(DateTime fromDate, DateTime toDate, int groupID, int bulkSize)
        {
            if (bulkSize < 1 || groupID < 1 || fromDate.CompareTo(toDate) > -1)
            {
                throw new ArgumentException("Incorrect input.");
            }
            this.locker = new object();
            this.bulkSize = bulkSize;
            this.fromDate = fromDate;
            this.toDate = toDate;
            this.groupID = groupID;
            this.index = 0;
            this.isInitialized = false;
            this.total = 0;
        }

        public bool Initialize()
        {
            bool res = true;
            lock (locker)
            {
                int totalAmtAccToDB = CatalogDAL.Create_SiteGuidsTableForUMMMigration(fromDate, toDate, groupID);
                if (totalAmtAccToDB == 0)
                {
                    res = false;
                }
                this.total = totalAmtAccToDB;
                this.isInitialized = true;
            }

            return res;
        }

        /*
         * returns false if there are no more site_guids table site_guids_for_umms_migration to consume.
         */
        public bool GetNextOffsets(ref int from, ref int to, ref int currIndex)
        {
            bool res = false;
            lock (locker)
            {
                if (index * bulkSize > total)
                {
                    res = false;
                }
                else
                {
                    res = true;
                    currIndex = index;
                    from = index * bulkSize + 1;
                    int tempTo = (++index * bulkSize);
                    if (tempTo > total)
                    {
                        to = total;
                    }
                    else
                    {
                        to = tempTo;
                    }
                }
            }

            return res;
        }

        public void Dispose()
        {
            lock (locker)
            {
                // drop the table site_guids_for_umms_migration
                isInitialized = false;
                if (!CatalogDAL.Drop_SiteGuidsTableForUMMMigration())
                {
                    log.Error("Error - Failed to drop umms_site_guids table in the DB.");
                }
            }
        }
    }
}
