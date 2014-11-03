using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchbaseMediaMarksFeeder
{
    internal class ChunkManager : IDisposable
    {
        private int index;
        private bool isInitialized;
        private int total;
        private int bulkSize;

        internal ChunkManager()
        {
            index = 0;
            isInitialized = false;
            total = 0;
        }

        public bool Initialize(int bulkSize)
        {
            lock (this)
            {
                if (bulkSize < 1)
                {
                    throw new ArgumentException("Incorrect value for bulkSize.");
                }
                /* 0. Check the bulk size is > 0
                 * 1. Invoke SP that creates site_guids_for_umms_migration
                 * 2. Populate it from users_media_mark table
                 * 3. Count num of rows in site_guids_for_umms_migration, and assign the value that returns to total.
                 * 4. set isInitialized=true, index = 0;
                 */
            }
            throw new NotImplementedException();
        }

        /*
         * returns false if there are no more site_guids table site_guids_for_umms_migration to consume.
         */
        public bool GetNextOffsets(ref int from, ref int to, ref int currIndex)
        {
            bool res = false;
            lock (this)
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
                    int tempTo = (++index * bulkSize) + 1;
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
            lock (this)
            {
                // drop the table site_guids_for_umms_migration
                isInitialized = false;

            }
        }
    }
}
