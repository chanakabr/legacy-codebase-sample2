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
        public object kwargs;

        #endregion

        public BaseCeleryData()
        {
            kwargs = new object();
            args = new List<object>();
        }

        public BaseCeleryData(string sID, string sTask, List<object> lArgs)
        {
            kwargs = new object();
            task = sTask;
            id = sID;       
            args = lArgs;
        }

        public BaseCeleryData(string sID, string sTask, params object[] lArgs)
        {
            kwargs = new object();
            task = sTask;
            id = sID;
            args = new List<object>();
            args.AddRange(lArgs);
        }
    }
}
