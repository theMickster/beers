using Beers.Application.Interfaces.Services.Base;
using Beers.Domain.Models;
using Beers.Domain.Models.Metadata;

namespace Beers.Application.Interfaces.Services;

public interface IReadBeerTypeService : IReadMetadataBaseService<BeerTypeModel>
{
}