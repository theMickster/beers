using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Beers.Domain.Models.Brewer;
using FluentValidation;

namespace Beers.Application.Validators.Brewer;

public class CreateBrewerValidator : BaseBrewerValidator<CreateBrewerModel>
{
    public CreateBrewerValidator(IReadBrewerService readBrewerService)
        : base(readBrewerService)
    {
        RuleFor(x => x)
            .MustAsync(async (x, cancellation)
                => await BrewerExistsAsync(x.Name).ConfigureAwait(false))
            .When(x => x?.Name != null)
            .WithMessage(ValidatorConstants.BrewerMustBeUnique)
            .OverridePropertyName("Name");
    }

    private async Task<bool> BrewerExistsAsync(string name)
    {
        var result = await ReadBrewerService.GetByNameAsync(name);
        return result == null;
    }
}

