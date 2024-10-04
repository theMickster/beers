using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Attributes;
using Beers.Domain.Models.Beer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class UpdateBeerService(
    IMapper mapper,
    IDbContextFactory<BeersDbContext> dbContextFactory,
    IValidator<UpdateBeerModel> validator) : IUpdateBeerService
{
    public Task<(ReadBeerModel, List<ValidationFailure>)> UpdateAsync(UpdateBeerModel inputModel)
    {


        throw new NotImplementedException();
    }
}
