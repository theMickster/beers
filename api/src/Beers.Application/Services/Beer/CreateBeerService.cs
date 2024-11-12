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

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class CreateBeerService (
    IMapper mapper,
    IValidator<CreateBeerModel> validator,
    IBeerHydrationService beerHydrationService,
    IDbContextFactory<BeersDbContext> dbContextFactory)
    : ICreateBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<CreateBeerModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IBeerHydrationService _beerHydrationService = beerHydrationService ?? throw new ArgumentNullException(nameof(beerHydrationService));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

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

        var inputEntity = await _beerHydrationService.HydrateEntity(inputModel);
        inputEntity.EntityType = PartitionKeyConstants.Beer;
        inputEntity.Id = Guid.NewGuid();
        inputEntity.CreatedBy = "the.system";
        inputEntity.ModifiedBy = "the.system";
        inputEntity.CreatedDate = DateTime.UtcNow;
        inputEntity.ModifiedDate = DateTime.UtcNow;
        inputEntity.IsDeletable = true;

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        context.BeerEntities.Add(inputEntity);
        await context.SaveChangesAsync();

        var outputEntity = await context.BeerEntities.SingleOrDefaultAsync(x => x.Id == inputEntity.Id);
        var outputModel = _mapper.Map<ReadBeerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, []) :
            (new ReadBeerModel(), [new ValidationFailure("Model", "Unable to retrieve the newly created beer model.")]);

    }
}
