using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Models.Brewer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Brewer;

[ServiceLifetimeScoped]
public sealed class CreateBrewerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<CreateBrewerModel> validator)
    : ICreateBrewerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IValidator<CreateBrewerModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Performs process of creating a new brewer.
    /// </summary>
    /// <param name="inputModel">the new brewer to create</param>
    /// <returns></returns>
    public async Task<(ReadBrewerModel, List<ValidationFailure>)> CreateAsync(CreateBrewerModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Count != 0)
        {
            return (new ReadBrewerModel(), validationResult.Errors);
        }

        var inputEntity = _mapper.Map<BrewerEntity>(inputModel);
        inputEntity.Id = Guid.NewGuid();
        inputEntity.BrewerId = inputEntity.Id;
        inputEntity.CreatedDate = DateTime.UtcNow;
        inputEntity.ModifiedDate = inputEntity.CreatedDate;
        inputEntity.EntityType = PartitionKeyConstants.Brewer;
        inputEntity.IsDeletable = true;

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.BrewerEntities.Add(inputEntity);
        await context.SaveChangesAsync();

        var outputEntity = await context.BrewerEntities.SingleOrDefaultAsync(x => x.Id == inputEntity.Id);
        var outputModel = _mapper.Map<ReadBrewerModel>(outputEntity);

        return outputModel != null ?
            (outputModel, new List<ValidationFailure>()) :
            (new ReadBrewerModel(), new List<ValidationFailure> { new("Model", "Unable to retrieve the newly created model.") });
    }

}
