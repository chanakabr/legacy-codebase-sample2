using Grpc.Core;
using Grpc.Core.Interceptors;
using KLogMonitor;
using System;
using System.Reflection;

namespace GrpcClientCommon
{
    public class TracingInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            using var tracer = RequestTracerFactory.NewTracer(context);
            return base.BlockingUnaryCall(request, context, continuation);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            using var tracer = RequestTracerFactory.NewTracer(context);
            return base.AsyncUnaryCall(request, context, continuation);
        }
    }


    /// <summary>
    /// This is here to avoid declearing TRequest, TResponse when initilizing a new Tracer in the client code
    /// </summary>
    public static class RequestTracerFactory
    {
        public static RequestTracer<Treq, Tresp> NewTracer<Treq, Tresp>(ClientInterceptorContext<Treq, Tresp> context)
            where Treq : class
            where Tresp : class
        {
            return new RequestTracer<Treq, Tresp>(context);
        }
    }

    public class RequestTracer<TRequest, TResponse> : IDisposable
        where TRequest : class
        where TResponse : class
    {
        private const string REQUEST_ID_HEADER_KEY = "x-kaltura-session-id";
        private readonly KMonitor _Monitor;


        

        public RequestTracer(ClientInterceptorContext<TRequest, TResponse> context)
        {
            var requestId = KLogger.GetRequestId();
            _Monitor = new KMonitor(Events.eEvent.EVENT_GRPC, "", context.Method.Name, requestId);
            _Monitor.Database = context.Host;
            context.Options.Headers.Add(REQUEST_ID_HEADER_KEY, requestId);
        }

        public void Dispose()
        {
            _Monitor.Dispose();
        }
    }
}