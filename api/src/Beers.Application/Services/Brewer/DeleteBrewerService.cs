using Beers.Application.Data;
using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Brewer;

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
        var entityToDelete = await context.BrewerEntities.FirstOrDefaultAsync(x => x.BrewerId == id);

        if (entityToDelete == null)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "BrewerId",
                    ErrorCode = ValidatorConstants.NotFoundErrorCode,
                    ErrorMessage = $"Unable to find brewer with id ${id}"
                }
            ]);
        }

        if (!entityToDelete.IsDeletable)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "BrewerId",
                    ErrorCode = ValidatorConstants.DeleteEntityNotAllowed,
                    ErrorMessage = $"Unable to delete brewer with id ${id} because it is not deletable. You may, however, attempt to set the record to inactive."
                }
            ]);
        }

        context.Remove(entityToDelete);
        var result = await context.SaveChangesAsync();

        if (result != 1)
        {
            return (false, [ new ValidationFailure("BrewerId", "Unable to delete the brewery entity.") ]);
        }

        return (true, []);
    }
}
