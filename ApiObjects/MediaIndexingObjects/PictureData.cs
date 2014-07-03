using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ApiObjects.MediaIndexingObjects
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
            serializer = new JavaScriptSerializer();
            args = new List<object>();
        }


        public PictureData(string sID, string sTask, List<object> lArgs)
        {
            serializer = new JavaScriptSerializer();
            task = sTask;
            id = sID;       
            args = lArgs;
        }

        public override string ToString()
        {
            return serializer.Serialize(this);
        }

    }  
}
