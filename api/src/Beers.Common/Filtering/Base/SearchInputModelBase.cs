namespace Beers.Common.Filtering.Base;

public abstract class SearchInputModelBase
{
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
