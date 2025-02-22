﻿using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Common.Attributes;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Entities.Slims;
using Beers.Domain.Models.Beer;
using Beers.Domain.Models.Metadata;

namespace Beers.Application.Services.Hydration;

[ServiceLifetimeScoped]
public sealed class BeerHydrationService(
    IReadBrewerService readBrewerService,
    IReadBeerTypeService readBeerTypeService,
    IReadBeerStyleService readBeerStyleService,
    IReadBeerCategoryService readBeerCategoryService) : IBeerHydrationService
{
    private readonly IReadBrewerService _readBrewerService = readBrewerService ?? throw new ArgumentNullException(nameof(readBrewerService));
    private readonly IReadBeerTypeService _readBeerTypeService = readBeerTypeService ?? throw new ArgumentNullException(nameof(readBeerTypeService));
    private readonly IReadBeerStyleService _readBeerStyleService = readBeerStyleService ?? throw new ArgumentNullException(nameof(readBeerStyleService));
    private readonly IReadBeerCategoryService _readBeerCategoryService = readBeerCategoryService ?? throw new ArgumentNullException(nameof(readBeerCategoryService));

    public async Task<BeerEntity> HydrateEntity(CreateBeerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        
        var brewer = await _readBrewerService.GetByIdAsync(model.BrewerId);
        var beerType = (await _readBeerTypeService.GetListAsync<BeerTypeModel>()).SingleOrDefault(x => x.Id == model.BeerTypeId);
        var beerStyles = await _readBeerStyleService.GetListAsync<BeerStyleModel>();
        var beerCategories = await _readBeerCategoryService.GetListAsync<BeerCategoryModel>();

        ArgumentNullException.ThrowIfNull(brewer);
        ArgumentNullException.ThrowIfNull(beerType);
        ArgumentNullException.ThrowIfNull(beerStyles);
        ArgumentNullException.ThrowIfNull(beerCategories);

        var entity = new BeerEntity
        {
            BrewerId = model.BrewerId,
            Name = model.Name,
            Description = model.Description,
            Image = model.Image,
            Sku = model.Sku,
            Brewer = new BrewerSlimEntity
            {
                Id = model.BrewerId,
                Name = brewer.Name,
                Website = brewer.Website
            },
            BeerType = new BeerTypeSlimEntity
            {
                MetadataId = model.BeerTypeId,
                Name = beerType.Name
            },
            IsDeletable = model.IsDeletable
        };

        if (model.Rating != null)
        {
            entity.Rating = new RatingEntity
            {
                Average = model.Rating.Average,
                ReviewCount = model.Rating.ReviewCount
            };
        }

        if (model.Pricing?.Count > 0)
        {
            entity.Pricing = [];
            foreach (var priceModel in model.Pricing)
            {
                entity.Pricing.Add( new PriceEntity
                {
                    Price = priceModel.Price,
                    Quantity = priceModel.Quantity,
                    Packaging = priceModel.Packaging,
                    UnitVolume = priceModel.UnitVolume
                });
            }
        }

        if (model.BeerCategories?.Count > 0)
        {
            entity.BeerCategories = [];
            model.BeerCategories.ForEach(categoryId =>
            {
                var category = beerCategories.FirstOrDefault(x => x.Id == categoryId);
                if (category != null)
                {
                    entity.BeerCategories.Add(new BeerCategorySlimEntity
                    {
                        MetadataId  = category.Id,
                        Name = category.Name
                    });
                }
            });
        }

        if (model.BeerStyles?.Count > 0)
        {
            entity.BeerStyles = [];
            model.BeerStyles.ForEach(styleId =>
            {
                var category = beerStyles.FirstOrDefault(x => x.Id == styleId);
                if (category != null)
                {
                    entity.BeerStyles.Add(new BeerStyleSlimEntity
                    {
                        MetadataId = category.Id,
                        Name = category.Name
                    });
                }
            });
        }

        return entity;
    }
}
