using Beers.Application.Interfaces.Services;
using Beers.Common.Constants;
using Beers.Domain.Models.Brewer;
using FluentValidation;

namespace Beers.Application.Validators.Brewer;

public sealed class UpdateBrewerValidator : BaseBrewerValidator<UpdateBrewerModel>
{
    public UpdateBrewerValidator(IReadBrewerService readBrewerService):
        base(readBrewerService)
    {
        RuleFor(x => x.BrewerId)
            .NotNull()
            .WithErrorCode("Rule-11").WithMessage(ValidatorConstants.BrewerIdIsNull);

        RuleFor(brewer => brewer)
            .MustAsync(async (brewer, cancellation)
                => await BrewerExistsAsync(brewer.BrewerId).ConfigureAwait(false))
            .When(x => x?.BrewerId != null)
            .WithMessage(ValidatorConstants.BrewerMustExist)
            .WithErrorCode("Rule-12")
            .OverridePropertyName("BrewerId");
    }

    private async Task<bool> BrewerExistsAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }
}
