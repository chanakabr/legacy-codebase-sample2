using Newtonsoft.Json.Linq;
using System.Linq;
using TCMClient;

namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public abstract class BaseConfig<T> : IBaseConfig
    {

        public abstract string TcmKey { get; }

        public void UpdateWithTcm(string[] tcmPath = null)
        {
            JToken token;
            if (tcmPath == null)
            {
                token = Settings.Instance.GetJsonString(new string[] { TcmKey });
            }
            else
            {
                token = Settings.Instance.GetJsonString(tcmPath.Concat(new string[] { TcmKey }).ToArray());
            }
            SetActualValues(token);
        }



        public abstract void SetActualValues(JToken token);


        public virtual void SetActualValue<T>(JToken token, BaseValue<T> defaultData)
        {

    /*        if (token == null)
            {
                //to delete this end write error
                throw new Exception();
                defaultData.ActualValue = defaultData.DefaultValue;
            }*/

            defaultData.ActualValue = token[defaultData.Key] == null ?
                defaultData.DefaultValue : token[defaultData.Key].ToObject<T>();

/*            if (defaultData.ActualValue == null)
            {
                throw new Exception();
            }*/

        }



    }
}
