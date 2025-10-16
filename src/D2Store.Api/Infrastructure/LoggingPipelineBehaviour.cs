using D2Store.Api.Shared;
using Mediator;

namespace D2Store.Api.Infrastructure;

public class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

    public LoggingPipelineBehaviour(ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Request {@RequestName}, {@DateTimeUtc}", typeof(TRequest).Name, DateTime.UtcNow);
        var result = await next(message, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogInformation("Request failure {@RequestName}, {@Error}, {@DateTimeUtc}", typeof(TRequest).Name, result.Error, DateTime.UtcNow);
        }
        _logger.LogInformation("Completed Request {@RequestName}, {@DateTimeUtc}", typeof(TRequest).Name, DateTime.UtcNow);
        return result;
    }
}
