using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.MediaIndexingObjects
{
    [Serializable]
    public class BaseCeleryData : QueueObject
    {
        #region Properties

        public string id;
        public string task;
        public List<object> args;

        #endregion

        public BaseCeleryData()
        {
            args = new List<object>();
        }


        public BaseCeleryData(string sID, string sTask, List<object> lArgs)
        {
            task = sTask;
            id = sID;       
            args = lArgs;
        }

        public BaseCeleryData(string sID, string sTask, params object[] lArgs)
        {
            task = sTask;
            id = sID;
            args = new List<object>();
            args.AddRange(lArgs);
        }
    }
}
