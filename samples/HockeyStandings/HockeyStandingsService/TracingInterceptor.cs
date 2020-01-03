using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace HockeyStandingsService
{
    public class TracingInterceptor : Interceptor
    {
        private readonly ILogger<TracingInterceptor> _logger;

        public TracingInterceptor(ILogger<TracingInterceptor> logger) => 
            _logger = logger;

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, 
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            LogTraceId(context);

            return continuation(request, context);
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, 
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogTraceId(context);

            return continuation(requestStream, context);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request, 
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, 
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogTraceId(context);

            return continuation(request, responseStream, context);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, 
            ServerCallContext context, 
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogTraceId(context);

            return continuation(requestStream, responseStream, context);
        }

        private void LogTraceId(ServerCallContext context)
        {
            var correlationId = context.RequestHeaders?.SingleOrDefault(header => header.Key == "x-correlation-id")?.Value;

            if (!string.IsNullOrWhiteSpace(correlationId))
                _logger.LogInformation($"Intercepting request with correlation ID: {correlationId}");
        }
    }
}