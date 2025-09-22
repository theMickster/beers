using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.NewsBlogPost;

[ServiceLifetimeScoped]
public sealed class CreateNewsBlogPostService(
    IMapper mapper,
    IValidator<CreateNewsBlogPostModel> validator,
    INewsBlogPostHydrationService newsBlogPostHydrationService,
    IDbContextFactory<BeersDbContext> dbContextFactory) : ICreateNewsBlogPostService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<CreateNewsBlogPostModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly INewsBlogPostHydrationService _newsBlogPostHydrationService = newsBlogPostHydrationService ?? throw new ArgumentNullException(nameof(newsBlogPostHydrationService));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Create a new NewsBlogPost
    /// </summary>
    public async Task<(ReadNewsBlogPostModel, List<ValidationFailure>)> CreateAsync(CreateNewsBlogPostModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel).ConfigureAwait(false);
        if (validationResult.Errors.Count != 0)
        {
            return (new ReadNewsBlogPostModel(), validationResult.Errors);
        }

        var inputEntity = await _newsBlogPostHydrationService.HydrateEntity(inputModel).ConfigureAwait(false);
        inputEntity.EntityType = PartitionKeyConstants.NewsBlogPost;
        inputEntity.Id = Guid.NewGuid();
        inputEntity.CreatedBy = "the.system";
        inputEntity.ModifiedBy = "the.system";
        inputEntity.CreatedDate = DateTime.UtcNow;
        inputEntity.ModifiedDate = inputEntity.CreatedDate;
        inputEntity.IsDeletable = true;

        await using var context = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        context.NewsBlogPostEntities.Add(inputEntity);
        await context.SaveChangesAsync().ConfigureAwait(false);

        var outputEntity = await context.NewsBlogPostEntities.SingleOrDefaultAsync(x => x.Id == inputEntity.Id).ConfigureAwait(false);
        var outputModel = _mapper.Map<ReadNewsBlogPostModel>(outputEntity);

        return outputModel != null ?
            (outputModel, []) :
            (new ReadNewsBlogPostModel(), [new ValidationFailure("Model", "Unable to retrieve the newly created news/blog post model.")]);
    }
}
