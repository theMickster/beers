using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;

namespace Beers.Application.Validators.Brewer;

public sealed class CreateBrewerReviewValidator : BaseBrewerReviewValidator<CreateBrewerReviewModel>
{
    public CreateBrewerReviewValidator(IReadBrewerService readBrewerService)
        : base(readBrewerService)
    {
        ValidateBrewer();
        ValidateReviewerName();
        ValidateTitle();
        ValidateComments();
        ValidateRating();
    }
}
