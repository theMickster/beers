using System.Globalization;
using Beers.Application.Interfaces.Services;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Beers.Domain.Models.Base;
using FluentValidation;

namespace Beers.Application.Validators.Brewer;

public abstract class BaseBrewerValidator<T> : AbstractValidator<T> where T : BaseBrewerModel
{
    protected readonly IReadBrewerService ReadBrewerService;

    protected BaseBrewerValidator(IReadBrewerService readBrewerService)
    {
        ReadBrewerService = readBrewerService;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidatorConstants.MessageNameEmpty)
            .MaximumLength(256)
            .WithMessage(ValidatorConstants.MessageNameLength);

        RuleFor(x => x.Headquarters)
            .NotEmpty()
            .WithMessage(ValidatorConstants.MessageHeadquartersEmpty)
            .MaximumLength(256)
            .WithMessage(ValidatorConstants.MessageHeadquartersLength);

        RuleFor(x => x.Website)
            .NotEmpty()
            .WithMessage(ValidatorConstants.MessageWebsiteEmpty)
            .MaximumLength(500)
            .WithMessage(ValidatorConstants.MessageWebsiteLength)
            .Must( (f, x) => f?.Website != null && f.Website.StartsWith("https://", true, CultureInfo.InvariantCulture) )
            .WithMessage(ValidatorConstants.MessageWebsiteHttps);

        RuleFor(x => x.FoundedIn)
            .Must(y => y >= 1500 && y <= DateTime.UtcNow.Year)
            .WithErrorCode("Rule-07").WithMessage(ValidatorConstants.MessageFoundedInBoundary);

        RuleFor(x => x.BreweryType)
            .NotNull()
            .WithErrorCode("Rule-08").WithMessage(ValidatorConstants.MessageBreweryTypeIsNull);

        RuleFor(x => x.BreweryType.Id)
            .NotEmpty()
            .When(x => x.BreweryType != null)
            .WithErrorCode("Rule-09").WithMessage(ValidatorConstants.MessageBreweryTypeIsNull)
            .OverridePropertyName("BreweryType");

        RuleFor(x => x.BreweryType.Name)
            .NotNull()
            .When(x => x.BreweryType != null)
            .WithErrorCode("Rule-10").WithMessage(ValidatorConstants.MessageBreweryTypeIsNull)
            .OverridePropertyName("BreweryType");
    }
}
