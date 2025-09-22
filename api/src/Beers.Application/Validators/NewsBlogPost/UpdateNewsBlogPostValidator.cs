using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Constants;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation;

namespace Beers.Application.Validators.NewsBlogPost;

public sealed class UpdateNewsBlogPostValidator : BaseNewsBlogPostValidator<UpdateNewsBlogPostModel>
{
    private readonly IReadNewsBlogPostService _readNewsBlogPostService;

    public UpdateNewsBlogPostValidator(
        IReadBrewerService readBrewerService,
        IReadNewsBlogPostService readNewsBlogPostService)
        : base(readBrewerService)
    {
        _readNewsBlogPostService = readNewsBlogPostService ?? throw new ArgumentNullException(nameof(readNewsBlogPostService));

        ValidateBrewer();
        ValidateTitle();
        ValidatePostType();
        ValidateConditionalFields();
        ValidateNewsBlogPostId();
        ValidatePostExists();
    }

    private void ValidateNewsBlogPostId()
    {
        RuleFor(x => x.NewsBlogPostId)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostIdIsNull);
    }

    private void ValidatePostExists()
    {
        RuleFor(x => x)
            .MustAsync(async (x, cancellation)
                => await PostExistsAsync(x.BrewerId, x.NewsBlogPostId).ConfigureAwait(false))
            .When(x => x.BrewerId != Guid.Empty && x.NewsBlogPostId != Guid.Empty)
            .WithMessage(ValidatorConstants.NewsBlogPostMustExist)
            .OverridePropertyName("NewsBlogPostId");
    }

    private async Task<bool> PostExistsAsync(Guid brewerId, Guid postId)
    {
        var result = await _readNewsBlogPostService.GetByIdAsync(brewerId, postId);
        return result != null && result.NewsBlogPostId != Guid.Empty;
    }
}
