using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Domain.Entities.Slims;
using Beers.Domain.Models.Brewer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class UpdateBrewerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<UpdateBrewerModel> validator)
    : IUpdateBrewerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IValidator<UpdateBrewerModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    
    /// <summary>
    /// Performs process of updating a brewer.
    /// </summary>
    /// <param name="inputModel">the brewer to update</param>
    /// <returns></returns>
    public async Task<(ReadBrewerModel, List<ValidationFailure>)> UpdateAsync(UpdateBrewerModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Count != 0)
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

        context.Update(entityToUpdate);
        await context.SaveChangesAsync();

        var outputEntity = await context.BrewerEntities.SingleOrDefaultAsync(x => x.Id == inputModel.BrewerId);
        var outputModel = _mapper.Map<ReadBrewerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, []) :
            (new ReadBrewerModel(), [new ValidationFailure("Model", "Unable to retrieve the newly created model.")]);
    }
}
