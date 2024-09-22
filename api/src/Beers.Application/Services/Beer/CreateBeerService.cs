using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Domain.Models.Beer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class CreateBeerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<CreateBeerModel> validator,
    IBeerHydrationService beerHydrationService)
    : ICreateBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IValidator<CreateBeerModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IBeerHydrationService _beerHydrationService = beerHydrationService ?? throw new ArgumentNullException(nameof(beerHydrationService));

    /// <summary>
    /// Performs process of creating a new beer.
    /// </summary>
    /// <param name="inputModel">the new beer to create</param>
    /// <returns></returns>
    public async Task<(ReadBeerModel, List<ValidationFailure>)> CreateAsync(CreateBeerModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Count != 0)
        {
            return (new ReadBeerModel(), validationResult.Errors);
        }

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var inputEntity = await _beerHydrationService.HydrateEntity(inputModel);
        inputEntity.EntityType = PartitionKeyConstants.Beer;

        var result = await context.AddBeerEntityAsync(inputEntity);

        if (result != HttpStatusCode.Created)
        {
            return (new ReadBeerModel(), new List<ValidationFailure> { new("Model", "Unable to create a beer entity.") });
        }

        var outputEntity = await context.BeerEntities.SingleOrDefaultAsync(x => x.Id == inputEntity.Id);
        var outputModel = _mapper.Map<ReadBeerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, new List<ValidationFailure>()) :
            (new ReadBeerModel(), new List<ValidationFailure> { new("Model", "Unable to retrieve the newly created beer model.") });

    }
}
