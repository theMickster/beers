using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Attributes;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.NewsBlogPost;

[ServiceLifetimeScoped]
public sealed class UpdateNewsBlogPostService(
    IMapper mapper,
    IValidator<UpdateNewsBlogPostModel> validator,
    INewsBlogPostHydrationService newsBlogPostHydrationService,
    IDbContextFactory<BeersDbContext> dbContextFactory) : IUpdateNewsBlogPostService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<UpdateNewsBlogPostModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly INewsBlogPostHydrationService _newsBlogPostHydrationService = newsBlogPostHydrationService ?? throw new ArgumentNullException(nameof(newsBlogPostHydrationService));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    public async Task<(ReadNewsBlogPostModel, List<ValidationFailure>)> UpdateAsync(UpdateNewsBlogPostModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel);

        if (validationResult.Errors.Count != 0)
        {
            return (new ReadNewsBlogPostModel(), validationResult.Errors);
        }

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entityToUpdate = await context.NewsBlogPostEntities.FirstOrDefaultAsync(
                                x => x.BrewerId == inputModel.BrewerId && x.Id == inputModel.NewsBlogPostId);

        if (entityToUpdate == null)
        {
            validationResult.Errors.Add(new ValidationFailure
            {
                PropertyName = "NewsBlogPostId",
                ErrorCode = "UpdateNewsBlogPost001",
                ErrorMessage = $"Unable to find news/blog post with id {inputModel.NewsBlogPostId}"
            });
            return (new ReadNewsBlogPostModel(), validationResult.Errors);
        }

        var inputEntity = await _newsBlogPostHydrationService.HydrateEntity(inputModel);

        entityToUpdate.ModifiedDate = DateTime.UtcNow;
        entityToUpdate.Title = inputEntity.Title;
        entityToUpdate.Body = inputEntity.Body;
        entityToUpdate.PostType = inputEntity.PostType;
        entityToUpdate.Tags = inputEntity.Tags;
        entityToUpdate.ImageUrls = inputEntity.ImageUrls;
        entityToUpdate.EventDate = inputEntity.EventDate;
        entityToUpdate.EventLocation = inputEntity.EventLocation;
        entityToUpdate.PublishedDate = inputEntity.PublishedDate;
        entityToUpdate.Author = inputEntity.Author;

        context.Update(entityToUpdate);
        await context.SaveChangesAsync();

        var outputModel = _mapper.Map<ReadNewsBlogPostModel>(entityToUpdate);

        return outputModel != null ?
            (outputModel, []) :
            (new ReadNewsBlogPostModel(), [new ValidationFailure("Model", "Unable to retrieve the updated model.")]);
    }
}
