using Beers.Application.Interfaces.Services.Beer;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models.Beer;

namespace Beers.Application.Validators.Beer;

public sealed class UpdateBeerValidator : BaseBeerValidator<UpdateBeerModel>
{
    public UpdateBeerValidator(
        IReadBeerService readBeerService,
        IReadBrewerService readBrewerService,
        IReadBeerCategoryService readBeerCategoryService,
        IReadBeerStyleService readBeerStyleService,
        IReadBeerTypeService readBeerTypeService) : base(readBeerService, readBrewerService)
    {
        
    }
}
