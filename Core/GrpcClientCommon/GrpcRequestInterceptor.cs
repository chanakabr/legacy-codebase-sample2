using Grpc.Core;
using Grpc.Core.Interceptors;
using Phx.Lib.Log;
using System;
using System.Reflection;
using Polly;
using Polly.Retry;

namespace GrpcClientCommon
{
    public class GrpcRequestInterceptor : Interceptor
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly RetryPolicy _grpcRetryPolicy;

        public GrpcRequestInterceptor(string address, int retryCount)
        {
            // Catch only transient errors that can be retried
            _grpcRetryPolicy = RetryPolicy
                .Handle<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded)
                .Retry(retryCount, (ex, attempt) =>
                {
                    _logger.Warn($"Error while calling grpc. address:[{address}]. retry [{attempt}/{retryCount}]", ex);
                });
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = GetNewContext(context);
            using var tracer = NewTracer(newContext);

            var resp = default(TResponse);
            _grpcRetryPolicy.Execute(() =>
            {
                resp = base.BlockingUnaryCall(request, newContext, continuation);
            });

            return resp;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = GetNewContext(context);
            using var tracer = NewTracer(newContext);

            var resp = default(AsyncUnaryCall<TResponse>);
            _grpcRetryPolicy.Execute(() =>
            {
                resp = base.AsyncUnaryCall(request, newContext, continuation);
            });

            return resp;
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
        private readonly KMonitor _monitor;

        public RequestTracer(ClientInterceptorContext<TRequest, TResponse> context)
        {
            var requestId = KLogger.GetRequestId();
            var groupId = KLogger.GetGroupId() ?? string.Empty;
            _monitor = new KMonitor(Events.eEvent.EVENT_GRPC, groupId)
            {
                UniqueID = requestId,
                Table = context.Method.Name,
                Database = context.Host
            };

            // TODO: Chage to PHX Liob Rest RequestContextConstants.SESSION_ID_KEY since this value moved from Klogger and
            // removed from phx.lib lof
            //context.Options.Headers.Add(Constants.SESSION_ID_KEY, requestId);
            context.Options.Headers.Add("x-kaltura-session-id", requestId);
        }

        public void Dispose()
        {
            _monitor.Dispose();
        }
    }
}