using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class DeleteBeerService(IDbContextFactory<BeersDbContext> dbContextFactory) : IDeleteBeerService
{
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Performs process of deleting a beer.
    /// </summary>
    /// <param name="id">the beer id to delete</param>
    /// <returns></returns>
    public async Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entityToDelete = await context.BeerEntities.FirstOrDefaultAsync(x => x.Id == id);

        if (entityToDelete == null)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "BeerId",
                    ErrorCode = ValidatorConstants.NotFoundErrorCode,
                    ErrorMessage = $"Unable to find beer with id ${id}"
                }
            ]);
        }

        if (!entityToDelete.IsDeletable)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "BeerId",
                    ErrorCode = ValidatorConstants.DeleteEntityNotAllowed,
                    ErrorMessage = $"Unable to delete beer with id ${id} because it is not deletable. You may, however, attempt to set the record to inactive."
                }
            ]);
        }

        context.Remove(entityToDelete);
        var result = await context.SaveChangesAsync();
        
        if (result != 1)
        {
            return (false, [new ValidationFailure("BeerId", "Unable to delete the beer entity.")]);
        }

        return (true, []);

    }
}
