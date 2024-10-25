using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Attributes;
using Beers.Domain.Models.Beer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class UpdateBeerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<UpdateBeerModel> validator) : IUpdateBeerService
{
    public async Task<(ReadBeerModel, List<ValidationFailure>)> UpdateAsync(UpdateBeerModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Count != 0)
        {
            return (new ReadBeerModel(), validationResult.Errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync();
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



        context.Update(entityToUpdate);
        await context.SaveChangesAsync();

        var outputEntity = await context.BeerEntities.SingleOrDefaultAsync(x => x.BrewerId == inputModel.BrewerId && x.Id == inputModel.BeerId);
        var outputModel = mapper.Map<ReadBeerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, []) :
            (new ReadBeerModel(), [new ValidationFailure("Model", "Unable to retrieve the newly created model.")]);
    }
}
