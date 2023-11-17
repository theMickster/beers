using Beers.Domain.Models.Brewer;
using FluentValidation;

namespace Beers.Application.Validators.Brewer;

public class CreateBrewerValidator : AbstractValidator<CreateBrewerModel>
{
    public CreateBrewerValidator()
    {
        RuleFor(  x => x.Name )
            .NotEmpty()
            .WithErrorCode("Rule-01").WithMessage(MessageNameEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-02").WithMessage(MessageNameLength);

        RuleFor(x => x.Headquarters)
            .NotEmpty()
            .WithErrorCode("Rule-03").WithMessage(MessageHeadquartersEmpty)
            .MaximumLength(256)
            .WithErrorCode("Rule-04").WithMessage(MessageHeadquartersLength);


        RuleFor(x => x.Website)
            .NotEmpty()
            .WithErrorCode("Rule-05").WithMessage(MessageWebsiteEmpty)
            .MaximumLength(500)
            .WithErrorCode("Rule-06").WithMessage(MessageWebsiteLength);

        RuleFor(x => x.FoundedIn)
            .Must(y => y >= 1500 && y <= DateTime.UtcNow.Year)
            .WithErrorCode("Rule-07").WithMessage(MessageFoundedInBoundary);
        
        RuleFor(x => x.BreweryType)
            .NotNull()
            .WithErrorCode("Rule-08").WithMessage(MessageTypeIsNull);

        RuleFor(x => x.BreweryType.Id)
            .NotNull()
            .When(x => x.BreweryType != null)
            .WithErrorCode("Rule-09").WithMessage(MessageTypeIsNull);

        RuleFor(x => x.BreweryType.Name)
            .NotNull()
            .When(x => x.BreweryType != null)
            .WithErrorCode("Rule-10").WithMessage(MessageTypeIsNull);
    }

    public static string MessageNameEmpty => "Name cannot be null, empty, or whitespace";

    public static string MessageNameLength => "Name cannot be longer than 256 characters";
    
    public static string MessageHeadquartersEmpty => "Headquarters cannot be null, empty, or whitespace";

    public static string MessageHeadquartersLength => "Headquarters cannot be longer than 256 characters";

    public static string MessageWebsiteEmpty => "Headquarters cannot be null, empty, or whitespace";

    public static string MessageWebsiteLength => "Headquarters cannot be longer than 500 characters";

    public static string MessageFoundedInBoundary => "Founded In year cannot before 1500 and after the current year.";

    public static string MessageTypeIsNull => "Brewery type is required";


}
