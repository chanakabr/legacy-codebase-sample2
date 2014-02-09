using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols
{
    public interface IProtocol
    {
        //TODO
        bool IsValidRequest();
        void PreSerialize();
        string PostSerialize(string serializedRequest);        
        string PreResponseProcess(string originalResponse);
        bool ProtocolUseZip{get;}
		bool IsWriteProtocol { get; }
    }    
}
