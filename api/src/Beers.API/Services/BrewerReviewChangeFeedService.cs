using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace Beers.API.Services;

/// <summary>
/// Background change feed processor that keeps Brewer rating aggregates in sync with Brewer review document changes.
/// </summary>
public sealed class BrewerReviewChangeFeedService(
    IServiceScopeFactory serviceScopeFactory,
    CosmosClient cosmosClient,
    CosmosDbConnectionSettings cosmosSettings,
    ILogger<BrewerReviewChangeFeedService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ??
        throw new ArgumentNullException(nameof(serviceScopeFactory));
    private readonly CosmosClient _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
    private readonly CosmosDbConnectionSettings _cosmosSettings = cosmosSettings ??
        throw new ArgumentNullException(nameof(cosmosSettings));
    private readonly ILogger<BrewerReviewChangeFeedService> _logger = logger ??
        throw new ArgumentNullException(nameof(logger));
    private ChangeFeedProcessor? _processor;

    /// <summary>
    /// Starts and runs the change feed processor until host shutdown.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token for host stop.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var database = _cosmosClient.GetDatabase(_cosmosSettings.DatabaseName);
        var monitoredContainer = database.GetContainer(CosmosContainerConstants.MainContainer);
        await database.CreateContainerIfNotExistsAsync(
            CosmosContainerConstants.ChangeFeedLeaseContainer,
            "/id",
            cancellationToken: stoppingToken);
        var leaseContainer = database.GetContainer(CosmosContainerConstants.ChangeFeedLeaseContainer);

        _processor = monitoredContainer
            .GetChangeFeedProcessorBuilder<JObject>(
                processorName: "BrewerReviewRatingAggregator",
                onChangesDelegate: HandleChangesAsync)
            .WithInstanceName(Environment.MachineName)
            .WithLeaseContainer(leaseContainer)
            .Build();

        _logger.LogInformation("Starting brewer review change feed processor.");
        await _processor.StartAsync();

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // host shutdown
        }
    }

    /// <summary>
    /// Stops the change feed processor during host shutdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful stop.</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
        {
            _logger.LogInformation("Stopping brewer review change feed processor.");
            await _processor.StopAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task HandleChangesAsync(
        ChangeFeedProcessorContext context,
        IReadOnlyCollection<JObject> changes,
        CancellationToken cancellationToken)
    {
        var brewerIds = changes
            .Where(x => string.Equals(x["EntityType"]?.ToString(), PartitionKeyConstants.BrewerReview, StringComparison.Ordinal))
            .Select(x => x["BrewerId"]?.ToObject<Guid>())
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (brewerIds.Count == 0)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var aggregationService = scope.ServiceProvider.GetRequiredService<IBrewerReviewAggregationService>();

        foreach (var brewerId in brewerIds)
        {
            await aggregationService.RecalculateBrewerRatingAsync(brewerId, cancellationToken);
        }
    }
}
