using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Common.Attributes;
using Beers.Domain.Models.Beer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class UpdateBeerService(
    IMapper mapper,
    IValidator<UpdateBeerModel> validator,
    IBeerHydrationService beerHydrationService,
    IDbContextFactory<BeersDbContext> dbContextFactory) : IUpdateBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<UpdateBeerModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IBeerHydrationService _beerHydrationService = beerHydrationService ?? throw new ArgumentNullException(nameof(beerHydrationService));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    public async Task<(ReadBeerModel, List<ValidationFailure>)> UpdateAsync(UpdateBeerModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Count != 0)
        {
            return (new ReadBeerModel(), validationResult.Errors);
        }

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entityToUpdate = await context.BeerEntities.FirstOrDefaultAsync(
                                x => x.BrewerId == inputModel.BrewerId && x.Id == inputModel.BeerId);

        if (entityToUpdate == null)
        {
            validationResult.Errors.Add(new ValidationFailure
            {
                PropertyName = "BeerId",
                ErrorCode = "UpdateBeer001",
                ErrorMessage = $"Unable to find beer with id ${inputModel.BeerId}"
            });
            return (new ReadBeerModel(), validationResult.Errors);
        }

        var inputEntity = await _beerHydrationService.HydrateEntity(inputModel);

        entityToUpdate.Name = inputEntity.Name;
        entityToUpdate.Description = inputEntity.Description;
        entityToUpdate.Sku = inputEntity.Sku;
        entityToUpdate.Image = inputEntity.Image;
        entityToUpdate.Pricing = inputEntity.Pricing;
        entityToUpdate.Rating = inputEntity.Rating;
        entityToUpdate.BeerType = inputEntity.BeerType;
        entityToUpdate.BeerCategories = inputEntity.BeerCategories;
        entityToUpdate.BeerStyles = inputEntity.BeerStyles;
        
        context.Update(entityToUpdate);
        await context.SaveChangesAsync();

        var outputModel = _mapper.Map<ReadBeerModel>(entityToUpdate);

        return outputModel != null ?
            (outputModel, []) :
            (new ReadBeerModel(), [new ValidationFailure("Model", "Unable to retrieve the newly created model.")]);
    }
}
