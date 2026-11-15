namespace Remedin.Application.Catalog.Search;

public sealed record Page
{
    public const int DefaultSize = 20;
    public const int MaximumSize = 50;

    private Page(int number, int size)
    {
        Number = number;
        Size = size;
    }

    public int Number { get; }

    public int Size { get; }

    public int Offset => (Number - 1) * Size;

    public static Page Of(int number, int size = DefaultSize) =>
        new(Math.Max(number, 1), Math.Clamp(size, 1, MaximumSize));
}

public sealed record PagedResults<T>(
    IReadOnlyList<T> Items,
    int Total,
    int PageNumber,
    int PageSize)
{
    private int Offset => (PageNumber - 1) * PageSize;

    public bool HasNext => Offset + Items.Count < Total;

    public bool HasPrevious => PageNumber > 1;

    public int FirstPosition => Items.Count == 0 ? 0 : Offset + 1;

    public int LastPosition => Items.Count == 0 ? 0 : Offset + Items.Count;

    public static PagedResults<T> Of(IReadOnlyList<T> items, int total, Page page) =>
        new(items, total, page.Number, page.Size);
}
