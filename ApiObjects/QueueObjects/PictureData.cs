using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ApiObjects
{
    public class PictureData : QueueObject
    {
        #region Properties       
        public string id;
        public string task;
        public List<object> args;
        #endregion

        public PictureData()
        {
            args = new List<object>();
        }


        public PictureData(string sID, string sTask, List<object> lArgs)
        {
            task = sTask;
            id = sID;       
            args = lArgs;
        }
    }  
}
