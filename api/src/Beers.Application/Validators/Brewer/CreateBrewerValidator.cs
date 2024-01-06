using Beers.Application.Interfaces.Services;
using Beers.Domain.Models.Brewer;

namespace Beers.Application.Validators.Brewer;

public class CreateBrewerValidator : BaseBrewerValidator<CreateBrewerModel>
{
    public CreateBrewerValidator(IReadBrewerService readBrewerService) :
        base(readBrewerService)
    {
    }
}
