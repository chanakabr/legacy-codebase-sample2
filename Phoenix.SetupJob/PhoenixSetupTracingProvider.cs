using System;
using OTT.Service.Kronos;

namespace Phoenix.SetupJob
{
    internal class PhoenixSetupTracingProvider : ITracingProvider
    {
        public string GetTraceId() =>  Guid.NewGuid().ToString();
    }
}