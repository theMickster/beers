using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models.Brewer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class CreateBrewerService : ICreateBrewerService
{
    private readonly IMapper _mapper;
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory;
    private readonly IValidator<CreateBrewerModel> _validator;
    private readonly CosmosClient _cosmosClient;
    private readonly IOptionsSnapshot<CosmosDbConnectionSettings> _cosmosDbSettings;

    public CreateBrewerService( IMapper mapper, IDbContextFactory<BeersDbContext> dbContextFactory, CosmosClient cosmosClient, IValidator<CreateBrewerModel> validator,
        IOptionsSnapshot<CosmosDbConnectionSettings> cosmosDbSettings)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _cosmosDbSettings = cosmosDbSettings ?? throw new ArgumentNullException(nameof(cosmosDbSettings));
    }

    /// <summary>
    /// Performs process of creating a new brewer.
    /// </summary>
    /// <param name="inputModel">the new brewer to create</param>
    /// <returns></returns>
    public async Task<(ReadBrewerModel, List<ValidationFailure>)> CreateAsync(CreateBrewerModel inputModel)
    {
        if (inputModel == null)
        {
            throw new ArgumentNullException(nameof(inputModel));
        }

        var validationResult = await _validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Any())
        {
            return (new ReadBrewerModel(), validationResult.Errors);
        }

        var inputEntity = _mapper.Map<BrewerEntity>(inputModel);
        inputEntity.Id = Guid.NewGuid();
        inputEntity.BrewerId = inputEntity.Id;
        inputEntity.CreatedDate = DateTime.UtcNow;
        inputEntity.ModifiedDate = inputEntity.CreatedDate;
        inputEntity.EntityType = PartitionKeyConstants.Brewer;

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var result = await context.AddBreweryEntityAsync(_cosmosClient, _cosmosDbSettings.Value, inputEntity);

        if (result != HttpStatusCode.Created)
        {
            return (new ReadBrewerModel(), new List<ValidationFailure> { new("Model", "Unable to create a brewery entity.") });
        }

        var outputEntity = await context.BrewerEntities.SingleOrDefaultAsync(x => x.Id == inputEntity.Id);
        var outputModel = _mapper.Map<ReadBrewerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, new List<ValidationFailure>()) :
            (new ReadBrewerModel(), new List<ValidationFailure> { new("Model", "Unable to retrieve the newly created model.") });
    }

}
