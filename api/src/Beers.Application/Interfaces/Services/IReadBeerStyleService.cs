﻿using Beers.Application.Interfaces.Services.Base;
using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services;

public interface IReadBeerStyleService : IBaseReadBeerMetadataService<BeerStyleModel>
{
}