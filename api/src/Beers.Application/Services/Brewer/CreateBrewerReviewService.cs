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
public sealed class CreateBrewerReviewService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<CreateBrewerReviewModel> validator)
    : ICreateBrewerReviewService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory =
        dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    private readonly IValidator<CreateBrewerReviewModel> _validator =
        validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<(ReadBrewerReviewModel, List<ValidationFailure>)> CreateAsync(CreateBrewerReviewModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var validationResult = await _validator.ValidateAsync(inputModel);
        if (validationResult.Errors.Count != 0)
        {
            return (new ReadBrewerReviewModel(), validationResult.Errors);
        }

        var inputEntity = _mapper.Map<BrewerReviewEntity>(inputModel);
        inputEntity.Id = Guid.NewGuid();
        inputEntity.EntityType = PartitionKeyConstants.BrewerReview;
        inputEntity.IsDeletable = true;
        inputEntity.CreatedBy = "the.system";
        inputEntity.ModifiedBy = "the.system";
        inputEntity.CreatedDate = DateTime.UtcNow;
        inputEntity.ModifiedDate = inputEntity.CreatedDate;

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.BrewerReviewEntities.Add(inputEntity);
        await context.SaveChangesAsync();

        var outputModel = _mapper.Map<ReadBrewerReviewModel>(inputEntity);
        return (outputModel, []);
    }
}
