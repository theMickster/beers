using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Beers.Domain.Enums;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation;

namespace Beers.Application.Validators.NewsBlogPost;

public sealed class CreateNewsBlogPostValidator : BaseNewsBlogPostValidator<CreateNewsBlogPostModel>
{
    public CreateNewsBlogPostValidator(IReadBrewerService readBrewerService)
        : base(readBrewerService)
    {
        ValidateBrewer();
        ValidateTitle();
        ValidatePostType();
        ValidateConditionalFields();
    }

    private void ValidateConditionalFields()
    {
        // EventAnnouncement requires EventDate and EventLocation
        RuleFor(x => x.EventDate)
            .NotNull()
            .WithMessage(ValidatorConstants.NewsBlogPostEventDateEmpty)
            .When(x => !string.IsNullOrWhiteSpace(x.PostType) &&
                       Enum.TryParse<NewsBlogPostType>(x.PostType, true, out var pt) && pt == NewsBlogPostType.EventAnnouncement);

        RuleFor(x => x.EventLocation)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostEventLocationEmpty)
            .When(x => !string.IsNullOrWhiteSpace(x.PostType) &&
                       Enum.TryParse<NewsBlogPostType>(x.PostType, true, out var pt) && pt == NewsBlogPostType.EventAnnouncement);

        // Body is only required for TextPost
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostBodyEmpty)
            .MaximumLength(4000)
            .WithMessage(ValidatorConstants.NewsBlogPostBodyLength)
            .When(x => !string.IsNullOrWhiteSpace(x.PostType) &&
                       Enum.TryParse<NewsBlogPostType>(x.PostType, true, out var pt) && pt == NewsBlogPostType.TextPost);

        // ImageGallery requires at least one ImageUrl
        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage(ValidatorConstants.NewsBlogPostImageUrlsEmpty)
            .When(x => !string.IsNullOrWhiteSpace(x.PostType) &&
                       Enum.TryParse<NewsBlogPostType>(x.PostType, true, out var pt) && pt == NewsBlogPostType.ImageGallery);
    }
}
