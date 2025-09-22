using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Attributes;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.NewsBlogPost;

[ServiceLifetimeScoped]
public sealed class ReadNewsBlogPostService(IMapper mapper, IDbContextFactory<BeersDbContext> dbContextFactory)
    : IReadNewsBlogPostService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Retrieve all news/blog posts for a brewer.
    /// </summary>
    public async Task<IReadOnlyList<ReadNewsBlogPostModel>> GetListAsync(Guid brewerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        var entities = await context.NewsBlogPostEntities
            .Where(x => x.BrewerId == brewerId)
            .OrderByDescending(x => x.PublishedDate ?? DateTime.MinValue)
            .ThenByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        return _mapper.Map<List<ReadNewsBlogPostModel>>(entities).AsReadOnly();
    }

    /// <summary>
    /// Retrieve a single news/blog post for a brewer.
    /// </summary>
    public async Task<ReadNewsBlogPostModel?> GetByIdAsync(Guid brewerId, Guid postId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        var entity = await context.NewsBlogPostEntities
            .FirstOrDefaultAsync(x => x.BrewerId == brewerId && x.Id == postId)
            .ConfigureAwait(false);

        return entity == null ? null : _mapper.Map<ReadNewsBlogPostModel>(entity);
    }
}
