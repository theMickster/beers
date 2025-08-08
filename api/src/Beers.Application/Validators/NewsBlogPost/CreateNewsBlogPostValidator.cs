using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.NewsBlogPost;

namespace Beers.Application.Validators.NewsBlogPost;

public sealed class CreateNewsBlogPostValidator : BaseNewsBlogPostValidator<CreateNewsBlogPostModel>
{
    public CreateNewsBlogPostValidator(IReadBrewerService readBrewerService)
        : base(readBrewerService)
    {
        ValidateBrewer();
        ValidateTitle();
        ValidateBody();
        ValidatePostType();
    }
}
