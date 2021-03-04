using Grpc.Core;
using Grpc.Core.Interceptors;
using KLogMonitor;
using System;

namespace GrpcClientCommon
{
    public class TracingInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = GetNewContext(context);
            using var tracer = NewTracer(newContext);
            return base.BlockingUnaryCall(request, newContext, continuation);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = GetNewContext(context);
            using var tracer = NewTracer(newContext);
            return base.AsyncUnaryCall(request, newContext, continuation);
        }
        
        private static ClientInterceptorContext<TRequest, TResponse> GetNewContext<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            return context.Options.Headers != null
                ? context
                : new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host,
                    context.Options.WithHeaders(new Metadata()));
        }

        private static RequestTracer<TRequest, TResponse> NewTracer<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            return new RequestTracer<TRequest, TResponse>(context);
        }
    }

    public class RequestTracer<TRequest, TResponse> : IDisposable
        where TRequest : class
        where TResponse : class
    {
        private const string REQUEST_ID_HEADER_KEY = "x-kaltura-session-id";
        private readonly KMonitor _monitor;
       
        public RequestTracer(ClientInterceptorContext<TRequest, TResponse> context)
        {
            var requestId = KLogger.GetRequestId();
            var groupId = KLogger.GetGroupId() ?? string.Empty;
            _monitor = new KMonitor(Events.eEvent.EVENT_GRPC, groupId, context.Method.Name, requestId)
            {
                Database = context.Host
            };
            context.Options.Headers.Add(REQUEST_ID_HEADER_KEY, requestId);
        }

        public void Dispose()
        {
            _monitor.Dispose();
        }
    }
}