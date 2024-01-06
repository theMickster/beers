using Beers.Application.Interfaces.Services;
using Beers.Domain.Models.Brewer;

namespace Beers.Application.Validators.Brewer;

public class CreateBrewerValidator(IReadBrewerService readBrewerService)
    : BaseBrewerValidator<CreateBrewerModel>(readBrewerService);
