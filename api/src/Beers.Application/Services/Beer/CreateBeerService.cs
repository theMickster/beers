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
public sealed class CreateBeerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<CreateBeerModel> validator)
    : ICreateBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IValidator<CreateBeerModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

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


        return (new ReadBeerModel(), new List<ValidationFailure>());
    }
}
