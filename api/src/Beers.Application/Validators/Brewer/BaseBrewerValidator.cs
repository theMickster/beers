using Beers.Application.Interfaces.Services;
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
            .WithErrorCode("Rule-01").WithMessage(ValidatorConstants.MessageNameEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-02").WithMessage(ValidatorConstants.MessageNameLength);

        RuleFor(x => x.Headquarters)
            .NotEmpty()
            .WithErrorCode("Rule-03").WithMessage(ValidatorConstants.MessageHeadquartersEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-04").WithMessage(ValidatorConstants.MessageHeadquartersLength);


        RuleFor(x => x.Website)
            .NotEmpty()
            .WithErrorCode("Rule-05").WithMessage(ValidatorConstants.MessageWebsiteEmpty)
            .MaximumLength(500)
            .WithErrorCode("Rule-06").WithMessage(ValidatorConstants.MessageWebsiteLength);

        RuleFor(x => x.FoundedIn)
            .Must(y => y >= 1500 && y <= DateTime.UtcNow.Year)
            .WithErrorCode("Rule-07").WithMessage(ValidatorConstants.MessageFoundedInBoundary);

        RuleFor(x => x.BreweryType)
            .NotNull()
            .WithErrorCode("Rule-08").WithMessage(ValidatorConstants.MessageTypeIsNull);

        RuleFor(x => x.BreweryType.Id)
            .NotNull()
            .When(x => x.BreweryType != null)
            .WithErrorCode("Rule-09").WithMessage(ValidatorConstants.MessageTypeIsNull);

        RuleFor(x => x.BreweryType.Name)
            .NotNull()
            .When(x => x.BreweryType != null)
            .WithErrorCode("Rule-10").WithMessage(ValidatorConstants.MessageTypeIsNull);
    }
}
