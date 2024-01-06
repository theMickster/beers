using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Common.Settings;
using Beers.Domain.Entities.Slims;
using Beers.Domain.Models.Brewer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class UpdateBrewerService : IUpdateBrewerService
{
    private readonly IMapper _mapper;
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory;
    private readonly CosmosClient _cosmosClient;
    private readonly IValidator<UpdateBrewerModel> _validator;
    private readonly IOptionsSnapshot<CosmosDbConnectionSettings> _cosmosDbSettings;

    public UpdateBrewerService(
        IMapper mapper, 
        IDbContextFactory<BeersDbContext> dbContextFactory, 
        CosmosClient cosmosClient, 
        IValidator<UpdateBrewerModel> validator,
        IOptionsSnapshot<CosmosDbConnectionSettings> cosmosDbSettings
        )
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _cosmosDbSettings = cosmosDbSettings ?? throw new ArgumentNullException(nameof(cosmosDbSettings));
    }

    /// <summary>
    /// Performs process of updating a brewer.
    /// </summary>
    /// <param name="inputModel">the brewer to update</param>
    /// <returns></returns>
    public async Task<(ReadBrewerModel, List<ValidationFailure>)> UpdateAsync(UpdateBrewerModel inputModel)
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

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entityToUpdate = await context.BrewerEntities.FirstOrDefaultAsync(x => x.BrewerId == inputModel.BrewerId);

        if (entityToUpdate == null)
        {
            validationResult.Errors.Add(new ValidationFailure
            {
                PropertyName = "BrewerId",
                ErrorCode = "UpdateBrewer001",
                ErrorMessage = $"Unable to find brewer with id ${inputModel.BrewerId}"
            });
            return (new ReadBrewerModel(), validationResult.Errors);
        }

        entityToUpdate.FoundedIn = inputModel.FoundedIn;
        entityToUpdate.Headquarters = inputModel.Headquarters;
        entityToUpdate.Name = inputModel.Name;
        entityToUpdate.Website = inputModel.Website;
        entityToUpdate.ModifiedDate = DateTime.UtcNow;
        entityToUpdate.BreweryType = new BreweryTypeSlimEntity()
        {
            MetadataId = inputModel.BreweryType.Id,
            Name = inputModel.BreweryType.Name
        };

        var result = await context.UpdateBreweryEntityAsync(_cosmosClient, _cosmosDbSettings.Value, entityToUpdate);
        
        if (result != HttpStatusCode.OK)
        {
            return (new ReadBrewerModel(), new List<ValidationFailure> { new("Model", "Unable to update the brewery entity.") });
        }

        var outputEntity = await context.BrewerEntities.SingleOrDefaultAsync(x => x.Id == inputModel.BrewerId);
        var outputModel = _mapper.Map<ReadBrewerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, new List<ValidationFailure>()) :
            (new ReadBrewerModel(), new List<ValidationFailure> { new("Model", "Unable to retrieve the newly created model.") });
    }
}
