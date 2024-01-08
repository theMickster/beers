using Beers.Application.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Domain.Models.Brewer;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class DeleteBrewerService (
    IDbContextFactory<BeersDbContext> dbContextFactory
    ) : IDeleteBrewerService
{
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Performs process of deleting a brewer.
    /// </summary>
    /// <param name="id">the brewer id to delete</param>
    /// <returns></returns>
    public async Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entityToUpdate = await context.BrewerEntities.FirstOrDefaultAsync(x => x.BrewerId == id);

        if (entityToUpdate == null)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "BrewerId", 
                    ErrorCode = "DeleteRule001",
                    ErrorMessage = $"Unable to find brewer with id ${id}"
                }
            ]);
        }

        var result = await context.DeleteBreweryEntityAsync(id);

        if (result != HttpStatusCode.NoContent)
        {
            return (false, [ new ValidationFailure("BrewerId", "Unable to delete the brewery entity.") ]);
        }

        return (true, []);
    }
}
