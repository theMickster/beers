using Beers.Application.Data;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.NewsBlogPost;

[ServiceLifetimeScoped]
public sealed class DeleteNewsBlogPostService(IDbContextFactory<BeersDbContext> dbContextFactory)
    : IDeleteNewsBlogPostService
{
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory =
        dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    public async Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid brewerId, Guid postId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        var entityToDelete = await context.NewsBlogPostEntities
            .FirstOrDefaultAsync(x => x.BrewerId == brewerId && x.Id == postId)
            .ConfigureAwait(false);

        if (entityToDelete == null)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "PostId",
                    ErrorCode = ValidatorConstants.NotFoundErrorCode,
                    ErrorMessage = $"Unable to find news/blog post with id {postId}"
                }
            ]);
        }

        if (!entityToDelete.IsDeletable)
        {
            return (false,
            [
                new ValidationFailure
                {
                    PropertyName = "PostId",
                    ErrorCode = ValidatorConstants.DeleteEntityNotAllowed,
                    ErrorMessage =
                        $"Unable to delete news/blog post with id {postId} because it is not deletable. You may, however, attempt to set the record to inactive."
                }
            ]);
        }

        context.Remove(entityToDelete);
        var result = await context.SaveChangesAsync().ConfigureAwait(false);

        if (result != 1)
        {
            return (false, [new ValidationFailure("PostId", "Unable to delete the news/blog post entity.")]);
        }

        return (true, []);
    }
}
