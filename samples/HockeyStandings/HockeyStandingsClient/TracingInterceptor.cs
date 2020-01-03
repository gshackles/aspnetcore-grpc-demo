using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace HockeyStandingsClient
{
    public class TracingInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            AddTraceId(ref context);

            return continuation(request, context);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            AddTraceId(ref context);

            return continuation(context);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            AddTraceId(ref context);

            return continuation(request, context);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            AddTraceId(ref context);

            return continuation(context);
        }

        private static void AddTraceId<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest: class 
            where TResponse: class
        {
            var traceId = Guid.NewGuid().ToString();

            Console.WriteLine($"Adding trace ID to request: {traceId}");

            if (context.Options.Headers == null)
            {
                var options = context.Options.WithHeaders(new Metadata());
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
            }

            context.Options.Headers.Add("x-correlation-id", traceId);
        }
    }
}