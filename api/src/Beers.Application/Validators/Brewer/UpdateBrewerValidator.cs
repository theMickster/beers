using Beers.Application.Interfaces.Services;
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
            .WithErrorCode("Rule-11").WithMessage(BrewerIdIsNull);

        RuleFor(brewer => brewer)
            .MustAsync(async (brewer, cancellation)
                => await BrewerMustExistAsync(brewer.BrewerId).ConfigureAwait(false))
            .When(x => x?.BrewerId != null)
            .WithMessage(BrewerMustExist)
            .WithErrorCode("Rule-12")
            .OverridePropertyName("BrewerId");
    }

    private async Task<bool> BrewerMustExistAsync(Guid brewerId)
    {
        var result = await ReadBrewerService.GetByIdAsync(brewerId);
        return result != null && result.BrewerId != Guid.Empty;
    }
}
