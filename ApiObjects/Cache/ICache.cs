using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ApiObjects.Cache
{
    [ServiceContract]
    public interface ICache<T>
    {
        [OperationContract]
        void Init();

        [OperationContract]
        T Get(string sID);
       
        [OperationContract]
        bool Insert(string sID, T oCache, DateTime? dtExpiresAt);

        [OperationContract]
        bool Insert(string sID, T group, DateTime? dtExpiresAt, ulong cas);
        
        [OperationContract]
        bool Update (string sID, T group, DateTime? dtExpiresAt);

        [OperationContract]
        bool Update(string sID, T group, DateTime? dtExpiresAt, ulong cas);
        
        [OperationContract]
        bool Delete(string sID);

    }
}
