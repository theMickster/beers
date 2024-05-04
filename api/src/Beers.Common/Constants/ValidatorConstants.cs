namespace Beers.Common.Constants;

public static class ValidatorConstants
{
    public static string MessageNameEmpty => "Name cannot be null, empty, or whitespace";

    public static string MessageNameLength => "Name cannot be longer than 256 characters";

    public static string MessageHeadquartersEmpty => "Headquarters cannot be null, empty, or whitespace";

    public static string MessageHeadquartersLength => "Headquarters cannot be longer than 256 characters";
    
    public static string MessageDescriptionEmpty => "Description cannot be null, empty, or whitespace";

    public static string MessageDescriptionLength => "Description cannot be longer than 256 characters";

    public static string MessageWebsiteEmpty => "Website cannot be null, empty, or whitespace";

    public static string MessageWebsiteLength => "Website cannot be longer than 500 characters";

    public static string MessageFoundedInBoundary => "Founded In year cannot before 1500 and after the current year.";

    public static string MessageTypeIsNull => "Brewery type is required";

    public static string BrewerIdIsNull => "Brewer Id is required";

    public static string BrewerMustExist => "Brewer does not exist for the supplied BrewerId";

    public static string BeerTypeMustExist => "Beer Type is required";
}
