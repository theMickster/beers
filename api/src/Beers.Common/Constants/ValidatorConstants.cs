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

    public static string BeerTypeIsNull => "Beer Type is required";
    
    public static string BeerTypeMustExist => "Beer Type is invalid";

    public static string BeerIdIsNull => "Beer Id is required";

    public static string BeerCategoriesMustExist => "Beer Categories list contains invalid item(s)";

    public static string BeerCategoriesIsNullOrEmpty => "Beer Categories list cannot be null nor empty";

    public static string BeerStylesMustExist => "Beer Styles list contains invalid item(s)";

    public static string BeerStylesIsNullOrEmpty => "Beer Styles list cannot be null nor empty";

    public static string BeerNameIsUnique => "Beer name cannot already exist in the database for a different record";
}
