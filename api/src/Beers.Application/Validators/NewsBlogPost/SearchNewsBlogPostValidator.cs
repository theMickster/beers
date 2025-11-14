using Beers.Common.Attributes;
using Beers.Common.Filtering.NewsBlogPost;
using FluentValidation;

namespace Beers.Application.Validators.NewsBlogPost;

[ServiceLifetimeScoped]
public sealed class SearchNewsBlogPostValidator : AbstractValidator<SearchInputNewsBlogPostModel>
{
    public SearchNewsBlogPostValidator()
    {
        RuleFor(x => x.DateRangeEnd)
            .Must((model, dateRangeEnd) => dateRangeEnd >= model.DateRangeStart)
            .When(x => x.DateRangeStart.HasValue && x.DateRangeEnd.HasValue)
            .WithMessage("DateRangeEnd must be greater than or equal to DateRangeStart.");
    }
}
