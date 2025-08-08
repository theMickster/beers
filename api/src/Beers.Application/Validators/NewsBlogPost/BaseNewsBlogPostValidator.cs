using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Beers.Domain.Enums;
using Beers.Domain.Models.Base;
using FluentValidation;

namespace Beers.Application.Validators.NewsBlogPost;

public abstract class BaseNewsBlogPostValidator<T>(IReadBrewerService readBrewerService) : AbstractValidator<T>
    where T : BaseNewsBlogPostModel
{
    protected readonly IReadBrewerService ReadBrewerService = readBrewerService;

    protected void ValidateBrewer()
    {
        RuleFor(x => x.BrewerId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.BrewerIdIsNull);

        RuleFor(x => x)
            .MustAsync(async (x, cancellation)
                => await BrewerExistsAsync(x.BrewerId).ConfigureAwait(false))
            .When(x => x.BrewerId != Guid.Empty)
            .WithMessage(ValidatorConstants.BrewerMustExist)
            .OverridePropertyName("BrewerId");
    }

    protected void ValidateTitle()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostTitleEmpty)
            .MaximumLength(256)
            .WithMessage(ValidatorConstants.NewsBlogPostTitleLength);
    }

    protected void ValidateBody()
    {
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostBodyEmpty)
            .MaximumLength(4000)
            .WithMessage(ValidatorConstants.NewsBlogPostBodyLength);
    }

    protected void ValidatePostType()
    {
        RuleFor(x => x.PostType)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostPostTypeEmpty)
            .Must(postType => Enum.GetNames<NewsBlogPostType>()
                                  .Contains(postType, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.PostType), ApplyConditionTo.CurrentValidator)
            .WithMessage(ValidatorConstants.NewsBlogPostPostTypeInvalid);
    }

    private async Task<bool> BrewerExistsAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }
}
